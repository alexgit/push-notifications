using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using NServiceBus;

namespace RealTime.TaskAllocationService.Terminal
{
    public class EndpointConfig : IConfigureThisEndpoint, AsA_Publisher, IWantCustomInitialization
    {
        public void Init()
        {
            NServiceBus.Configure.With()
                        .Log4Net()
                        .DefaultBuilder()
                        .XmlSerializer()
                        .MsmqTransport()
                            .IsTransactional(true)
                            .PurgeOnStartup(false)                        
                        .UnicastBus()
                            .ImpersonateSender(false)
                        .CreateBus()
                        .Start();
        }        
    }
}
