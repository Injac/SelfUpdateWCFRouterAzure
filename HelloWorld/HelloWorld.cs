using ContractAssembly;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HelloWorld
{
    class HelloWorld : IHelloWorldService
    {
        /// <summary>
        /// Says the hello to.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <returns></returns>
        public string SayHelloTo(string name)
        {
            return string.Format("Hello {0}!", name);
        }
    }
}
