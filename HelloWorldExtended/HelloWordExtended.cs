using ContractAssembly;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HelloWorldExtended
{
    public class HelloWordExtended : ISayHelloWorldExtended
    {
        public string SayHelloToExtended(string name, string additionalPhrase)
        {
            return String.Format("Hello {0} ! this is the additional phrase: {1}", name, additionalPhrase);
        }
    }
}
