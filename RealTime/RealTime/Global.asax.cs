using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using SignalR;
using RealTime.EndPoints;
using RealTime.MessageHandlers;
using NServiceBus;
using Castle.Windsor;
using System.Web.Security;
using Newtonsoft.Json;
using RealTime.Models;
using Castle.MicroKernel.Registration;
using System.Reflection;
using log4net.Appender;
using log4net.Core;

namespace RealTime
{
    // Note: For instructions on enabling IIS6 or IIS7 classic mode, 
    // visit http://go.microsoft.com/?LinkId=9394801

    public class MvcApplication : System.Web.HttpApplication
    {
        public IBus Bus { get; private set; }
        public IWindsorContainer Container { get; private set; }

        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new HandleErrorAttribute());
        }

        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            routes.MapConnection<UserTaskEndpoint>("relay", "relay/{*operation}");

            routes.MapHttpRoute(
                name: "DefaultApi",
                routeTemplate: "api/{controller}/{id}",
                defaults: new { id = RouteParameter.Optional }
            );

            routes.MapRoute(
                name: "Default",
                url: "{controller}/{action}/{id}",
                defaults: new { controller = "Home", action = "Index", id = UrlParameter.Optional }
            );            
        }

        protected void Application_Start()
        {
            ConfigureIoC();
            ConfigureNServiceBus();
            
            ControllerBuilder.Current.SetControllerFactory(new MyControllerFactory(Container));

            AreaRegistration.RegisterAllAreas();

            RegisterGlobalFilters(GlobalFilters.Filters);
            RegisterRoutes(RouteTable.Routes);

            BundleTable.Bundles.RegisterTemplateBundles();            
        }

        protected void Application_AuthenticateRequest(Object sender, EventArgs e) 
        {
            HttpCookie authCookie = Request.Cookies[FormsAuthentication.FormsCookieName];

            if (authCookie != null)
            {
                FormsAuthenticationTicket authTicket = FormsAuthentication.Decrypt(authCookie.Value);
                var userData = JsonConvert.DeserializeObject<UserSerializeModel>(authTicket.UserData);
                
                if(userData != null) 
                {
                    User newUser = new User(userData.Id, userData.Username);
                    HttpContext.Current.User = newUser;                
                }
            }
        }

        private void ConfigureIoC()
        {
            Container = new WindsorContainer();

            var connectionLookup = new ConnectionLookup();
            var taskNotifier = new TaskNotifier(connectionLookup);
            var userAccountService = new DummyUserAccountService();
            var taskDistributor = new TaskDistributor(taskNotifier, userAccountService);
                        
            Container.Register(Component.For<IUserAccountService>().Instance(userAccountService).LifestyleSingleton());
            Container.Register(Component.For<INotifyUsersOfTasks>().Instance(taskNotifier).LifestyleSingleton());
            Container.Register(Component.For<TaskDistributor>().Instance(taskDistributor).LifestyleSingleton());            

            Container.Register(Component.For<IControllerActivator>()
                .UsingFactoryMethod<MyControllerActivator>((a, b) => new MyControllerActivator(Container))
                    .LifeStyle.Is(Castle.Core.LifestyleType.Singleton));

            Container.Register(Component.For<IControllerFactory>()
                .UsingFactoryMethod<MyControllerFactory>((a, b) => new MyControllerFactory(Container))
                    .LifeStyle.Is(Castle.Core.LifestyleType.Singleton));            

            Container.Register(AllTypes.FromThisAssembly().BasedOn<IController>().LifestyleTransient());

            GlobalHost.DependencyResolver.Register(typeof(TaskDistributor), () => taskDistributor);
            GlobalHost.DependencyResolver.Register(typeof(ConnectionLookup), () => connectionLookup);
            GlobalHost.DependencyResolver.Register(typeof(IBus), () => Bus);
            GlobalHost.DependencyResolver.Register(typeof(UserTaskEndpoint), () => new UserTaskEndpoint(taskDistributor, connectionLookup, Bus));                      
        }

        private void ConfigureNServiceBus()
        {
            var appender = new RollingFileAppender 
            {
                File = @"c:\log.txt",                
                MaximumFileSize = "100MB",
                MaxSizeRollBackups = 5,
                Threshold = Level.Debug
            };

            Bus = NServiceBus.Configure.With()
                                        .Log4Net(appender)
                                        .CastleWindsorBuilder(Container)
                                        .XmlSerializer()
                                        .MsmqTransport()
                                            .IsTransactional(false)
                                            .PurgeOnStartup(true)                                        
                                        .DefineEndpointName("realtime")
                                        .UnicastBus()
                                            .ImpersonateSender(false)
                                            .LoadMessageHandlers()
                                        .CreateBus()
                                        .Start();
        }        
        
        public class MyControllerActivator : IControllerActivator 
        {
            private IWindsorContainer container;

            public MyControllerActivator(IWindsorContainer container)
            {
                this.container = container;
            }

            public IController Create(RequestContext requestContext, Type controllerType)
            {
                return container.Resolve(controllerType) as IController;
            }
        }

        public class MyControllerFactory : DefaultControllerFactory
        {
            private IWindsorContainer container;

            public MyControllerFactory(IWindsorContainer container)
            {
                this.container = container;
            }

            protected override IController GetControllerInstance(RequestContext requestContext, Type controllerType)
            {
                return container.Resolve(controllerType) as IController;
            }           
        }        
    }
}