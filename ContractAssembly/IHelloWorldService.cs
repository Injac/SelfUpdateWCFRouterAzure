using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace ContractAssembly
{
    [ServiceContract]
    public interface IHelloWorldService
    {
        /// <summary>
        /// Says the hello to.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <returns></returns>
        [OperationContract]
        string SayHelloTo(string name);
                
    }
}
