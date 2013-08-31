using Microsoft.Experience.CloudFx.Framework.Configuration;
using Microsoft.Experience.CloudFx.Framework.Messaging;
using Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiceMessenger
{
    public class Messenger
    {
        private RouteMeModel model;
        
        /// <summary>
        /// Initializes a new instance of the <see cref="Messenger" /> class.
        /// </summary>
        /// <param name="servcieModel">The servcie model.</param>
        public Messenger(string endPointAddress, string contracName, string assemblyName, string serviceName)
        {
            this.model = new RouteMeModel()
            {
                ContractName = contracName,
                EndPointAddress = endPointAddress,
                FullAssemblyName = assemblyName,
                ServiceName = serviceName
            };
        }

        /// <summary>
        /// Setups the service bus.
        /// </summary>
        public void SendMessageToRouter()
        {
            //Setup the service subscription channel
           
            var config = CloudApplicationConfiguration.Current.GetSection<ServiceBusConfigurationSection>(ServiceBusConfigurationSection.SectionName);

            var routerServiceCtx = new RoutingMessageContext { To = "ServiceRouter" };
            
            using (var pubSubChannel = new ServiceBusPublishSubscribeChannel(config.Endpoints.Get(config.DefaultEndpoint)))
            {
                pubSubChannel.Settings.MessageTimeToLive = TimeSpan.FromSeconds(120);
                

                //publish the message
                pubSubChannel.Publish(this.model, routerServiceCtx);
            }
        }
    }
}