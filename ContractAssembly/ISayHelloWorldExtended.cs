using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace ContractAssembly
{
    [ServiceContract]
    public interface ISayHelloWorldExtended
    {
        /// <summary>
        /// Says the hello to extended.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="additionalPhrase">The additional phrase.</param>
        /// <returns></returns>
        [OperationContract]
        string SayHelloToExtended(string name, string additionalPhrase);

    }
}
