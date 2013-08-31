using Microsoft.Experience.CloudFx.Framework.Configuration;
using Microsoft.Experience.CloudFx.Framework.Messaging;
using Microsoft.Experience.CloudFx.Framework.Storage;
using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.ServiceRuntime;
using Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reactive;
using System.Reflection;
using System.ServiceModel;
using System.ServiceModel.Description;
using System.ServiceModel.Dispatcher;
using System.ServiceModel.Routing;
using System.Text;
using System.Threading.Tasks;

namespace SelfUpdatingServiceRouter.Extensions
{
    class RouterUpdateExtension : IExtension<ServiceHostBase>, IDisposable
    {

        private RoleInstanceEndpoint endpointAzure;

        ServiceHostBase owner;

        IObserver<RouteMeModel> modelObserver;

        ServiceBusPublishSubscribeChannel pubSubChannel;

        List<ServiceEndpoint> serviceEndPoints;

        //The routing configuration to use
        RoutingConfiguration rc;

        /// <summary>
        /// Attaches the specified owner.
        /// </summary>
        /// <param name="owner">The owner.</param>
        public void Attach(ServiceHostBase owner)
        {
            this.owner = owner;

            //This could be done using the owner, you can read it near the bottom of this code-file
            endpointAzure = RoleEnvironment.CurrentRoleInstance.InstanceEndpoints["RoutingServiceMain"];

            //start the fun
            this.Init();
        }

        /// <summary>
        /// Detaches the specified owner.
        /// </summary>
        /// <param name="owner">The owner.</param>
        public void Detach(ServiceHostBase owner)
        {
            this.Dispose();
        }

        public void Dispose()
        {
            //Dispose all the stuff
        }

        /// <summary>
        /// Inits the Service Bus stuff
        /// </summary>
        private void Init()
        {
            this.serviceEndPoints = new List<ServiceEndpoint>();
            this.rc = new RoutingConfiguration();
            this.AddRoutingEntries();
            this.SetupServiceBus();
        }

        /// <summary>
        /// Adds the routing entries.
        /// </summary>
        private void AddRoutingEntries()
        {
            using (var ctx = new RouteContext())
            {
                if (ctx.Services.Count() > 0)
                {
                    foreach (var entry in ctx.Services)
                    {
                        //Add a route for the service
                        AddServiceBusEntry(entry);
                    }
                }
            }
        }

        /// <summary>
        /// Setups the service bus.
        /// </summary>
        private void SetupServiceBus()
        {
            //Setup the service subscription channel
            var config = CloudApplicationConfiguration.Current.GetSection<ServiceBusConfigurationSection>(ServiceBusConfigurationSection.SectionName);

            pubSubChannel = new ServiceBusPublishSubscribeChannel(config.Endpoints.Get(config.DefaultEndpoint));
       
            CreateSubscriptionForService(pubSubChannel, "RoutingService");
        }

        /// <summary>
        /// Creates the subscription for service.
        /// </summary>
        /// <param name="pubSubChannel">The pub sub channel.</param>
        /// <param name="p">The p.</param>
        private void CreateSubscriptionForService(ServiceBusPublishSubscribeChannel pubSubChannel, string serviceName)
        {
            //Define a filter to receive only messages that have a specific "To" property
            var filter = FilterExpressions.GroupOr(
                FilterExpressions.MatchTo(serviceName),
                FilterExpressions.MatchTo("ServiceRouter"));
            //Now we need to create an observer, that will check for new incoming messages
            modelObserver = Observer.Create<RouteMeModel>(msg =>
            {
                var exists = CheckIfRoutingEntryExists(msg);

                if (!exists)
                {
                    AddNewServiceEntry(msg);
                }
            });

            pubSubChannel.Subscribe(serviceName,modelObserver,filter);
        }

        /// <summary>
        /// Adds the new service entry.
        /// </summary>
        /// <param name="msg">The MSG.</param>
        private void AddNewServiceEntry(RouteMeModel msg)
        {
            using (var ctx = new RouteContext())
            {
                ctx.ChangeTracker.DetectChanges();
                msg.SericeUID = Guid.NewGuid().ToString();
                ctx.Services.Add(msg);
                ctx.SaveChanges();
                //Add a route for the service
                AddServiceBusEntry(msg);
            }
        }

        /// <summary>
        /// Checks if routing entry exists.
        /// </summary>
        /// <param name="msg">The MSG.</param>
        /// <returns></returns>
        private bool CheckIfRoutingEntryExists(RouteMeModel msg)
        {
            using (var ctx = new RouteContext())
            {
                var entry = (from service in ctx.Services
                            where service.ServiceName.Equals(msg.ServiceName) && msg.ContractName.Equals(msg.ContractName)
                            select service).FirstOrDefault();
                if(entry == null)
                {
                    return false;
                }
                else
                {
                    return true;
                }
            }
        }

      
        /// <summary>
        /// Adds the service bus entry.
        /// </summary>
        /// <param name="message">The message.</param>
        private void AddServiceBusEntry(RouteMeModel message)
        {
            //Load the contract assembly from blob storage
            //Get the current type of contract to add to the client endpoint
            var storageConnection = CloudConfigurationManager.GetSetting("Microsoft.WindowsAzure.Plugins.Diagnostics.ConnectionString");

            var cloudStorage = new ReliableCloudBlobStorage(StorageAccountInfo.Parse(storageConnection));

            var contractAssemblyContainer = CloudConfigurationManager.GetSetting("AssemblyContainerName");

            var contractAssemblyName = CloudConfigurationManager.GetSetting("ContractAssemblyName");

            byte[] data = null;

            using (MemoryStream mstream = new MemoryStream())
            {
                var gotIt = cloudStorage.Get(contractAssemblyContainer, contractAssemblyName, mstream);
                if (gotIt)
                {
                    //Get the byte content. This methods does not care about the position
                    data = mstream.ToArray();
                }
            }

            //Now we load the contract assembly
            var assembly = Assembly.Load(data);

            Type contractType = assembly.GetType(message.ContractName);

            //The contract description we use 
            var conDesc = ContractDescription.GetContract(contractType);
                        
            var HTTPbinding = new BasicHttpBinding();

            var currentServiceEndPoint = new ServiceEndpoint(
                conDesc,
                HTTPbinding,
                new EndpointAddress(message.EndPointAddress));

            currentServiceEndPoint.Name = message.ServiceName;
           
            var routerMainEndpoint = owner.Description.Endpoints.Where(ep => ep.Name == "RouterMain").FirstOrDefault();

            var conDescRouter = ContractDescription.GetContract(typeof(IRequestReplyRouter));
            var rEndPoint = new ServiceEndpoint(conDescRouter,new BasicHttpBinding(), new EndpointAddress( routerMainEndpoint.Address.Uri.OriginalString +"/" + message.ServiceName));
            rEndPoint.Name = message.ServiceName;
            
            this.owner.AddServiceEndpoint(rEndPoint);

            var addressFilter = new EndpointAddressMessageFilter(new EndpointAddress(routerMainEndpoint.Address.Uri.OriginalString + "/" + message.ServiceName));

            //We don't want to route on headers only
            rc.RouteOnHeadersOnly = false;

            //Add the filter table
            rc.FilterTable.Add(addressFilter, new List<ServiceEndpoint>() { currentServiceEndPoint });

            //Apply the dynamic configuration
            this.owner.Extensions.Find<RoutingExtension>().ApplyConfiguration(rc);
        }

        /// <summary>
        /// Checks if end point was added.
        /// </summary>
        /// <param name="host">The host.</param>
        /// <param name="endpointAddress">The endpoint address.</param>
        /// <returns></returns>
        private static bool CheckIfEndPointWasAdded(ServiceHostBase host, string endpointAddress)
        {
            bool isPresent = false;

            foreach (var endpoint in host.Description.Endpoints.ToList())
            {
                if (endpoint.ListenUri.AbsoluteUri.Equals(endpointAddress))
                {
                    isPresent = true;
                    break;
                }
            }

            return isPresent;
        }
    }
}