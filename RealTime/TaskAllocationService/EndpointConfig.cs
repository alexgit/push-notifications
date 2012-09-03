using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Castle.Core;
using Castle.Windsor;
using Castle.MicroKernel.Registration;

using NServiceBus;

namespace TaskAllocationService
{
    public class EndpointConfig : IConfigureThisEndpoint, AsA_Publisher, IWantCustomInitialization
    {
        public void Init()
        {
            var container = SetupContainer();

            NServiceBus.Configure.With()
                        .Log4Net()
                        .CastleWindsorBuilder(container)
                        .XmlSerializer()
                        .MsmqTransport()
                            .IsTransactional(true)
                            .PurgeOnStartup(false)                        
                        .UnicastBus()
                            .ImpersonateSender(false)
                        .CreateBus()
                        .Start();
        }

        private IWindsorContainer SetupContainer() 
        {            
            var container = new WindsorContainer();

            container.Register(Component.For<ITeamService>().ImplementedBy<TeamServiceImpl>().LifeStyle.Is(LifestyleType.Singleton));

            return container;
        }
    }
}
