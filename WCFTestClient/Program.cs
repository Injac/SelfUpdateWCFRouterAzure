using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ServiceModel;
using ContractAssembly;

namespace WCFTestClient
{
    class Program
    {
        static void Main(string[] args)
        {

            //Setup the channel factories (no proxy generation using service references)
            //Since we've got this router thing going on, we will use the router url, extending it
            //with the respective servcie name
            ChannelFactory<IHelloWorldService> helloWOrldChannel = new ChannelFactory<IHelloWorldService>(new BasicHttpBinding(),"http://localhost:10101/ServiceRouter/HelloWorld");
            ChannelFactory<ISayHelloWorldExtended> helloWOrldExtChannel = new ChannelFactory<ISayHelloWorldExtended>(new BasicHttpBinding(),"http://localhost:10101/ServiceRouter/HelloWorldExtended");

            //Create the channels...
            var hWorldChannel = helloWOrldChannel.CreateChannel();
            var hWorldExtChannel = helloWOrldExtChannel.CreateChannel();

            //Call the service methods ....
            var hWorldMessage = hWorldChannel.SayHelloTo("Hello Dude!");
            var extHWorldMessage = hWorldExtChannel.SayHelloToExtended("Hello Dude!", "Well done!");

            helloWOrldChannel.Close();
            helloWOrldExtChannel.Close();

        }
    }
}
