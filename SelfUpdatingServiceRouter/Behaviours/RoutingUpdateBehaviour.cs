using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel.Configuration;
using System.ServiceModel.Description;
using System.Text;
using System.Threading.Tasks;
using SelfUpdatingServiceRouter.Extensions;

namespace SelfUpdatingServiceRouter.Behaviours
{
    public class RoutingUpdateBehaviour : BehaviorExtensionElement, IServiceBehavior
    {

        /// <summary>
        /// Gets the type of behavior.
        /// </summary>
        /// <returns>The type of behavior.</returns>
        /// <value></value>
        public override Type BehaviorType
        {
            get { return typeof(RoutingUpdateBehaviour); }
        }

        /// <summary>
        /// Creates a behavior extension based on the current configuration settings.
        /// </summary>
        /// <returns>The behavior extension.</returns>
        protected override object CreateBehavior()
        {
            return new RoutingUpdateBehaviour();
        }

        /// <summary>
        /// Adds the binding parameters.
        /// </summary>
        /// <param name="serviceDescription">The service description.</param>
        /// <param name="serviceHostBase">The service host base.</param>
        /// <param name="endpoints">The endpoints.</param>
        /// <param name="bindingParameters">The binding parameters.</param>
        public void AddBindingParameters(ServiceDescription serviceDescription, System.ServiceModel.ServiceHostBase serviceHostBase, System.Collections.ObjectModel.Collection<ServiceEndpoint> endpoints, System.ServiceModel.Channels.BindingParameterCollection bindingParameters)
        {
            //Not implemented
        }

        /// <summary>
        /// Provides the ability to change run-time property values or insert custom
        /// extension objects such as error handlers, message or parameter interceptors,
        /// security extensions, and other custom extension objects.
        /// </summary>
        /// <param name="serviceDescription">The service description.</param>
        /// <param name="serviceHostBase">The host that is currently being built.</param>
        public void ApplyDispatchBehavior(ServiceDescription serviceDescription, System.ServiceModel.ServiceHostBase serviceHostBase)
        {
            RouterUpdateExtension updateExtension = new RouterUpdateExtension();
            serviceHostBase.Extensions.Add(updateExtension);
        }

        /// <summary>
        /// Provides the ability to inspect the service host and the service description
        /// to confirm that the service can run successfully.
        /// </summary>
        /// <param name="serviceDescription">The service description.</param>
        /// <param name="serviceHostBase">The service host that is currently being constructed.</param>
        public void Validate(ServiceDescription serviceDescription, System.ServiceModel.ServiceHostBase serviceHostBase)
        {
           //Not implemented
        }
    }
}