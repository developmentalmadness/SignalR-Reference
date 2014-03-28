using System;
using System.Threading.Tasks;
using Microsoft.Owin;
using Owin;
using Microsoft.Owin.Cors;
using Microsoft.AspNet.SignalR;
using Microsoft.Practices.Unity;
using SignalRHost.Utility;
using System.Reflection;
using Microsoft.AspNet.SignalR.Infrastructure;

[assembly: OwinStartup(typeof(SignalRHost.Startup))]

namespace SignalRHost
{
	public class Startup
	{
		public void Configuration(IAppBuilder app)
		{
			var container = new UnityContainer();

			// hook into SignalR DI chain and register it as well
			var unity = new UnityDependencyResolver(container);
			container.RegisterInstance<IDependencyResolver>(unity);
			GlobalHost.DependencyResolver = unity;

			// map interfaces to implementations (one-to-one only; one-to-many impls will break this)
			container.RegisterTypes(
				AllClasses.FromAssemblies(Assembly.Load("SignalRHost, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null")),
				WithMappings.FromMatchingInterface,
				WithName.Default
			);

			// pre-resolve assembly types
			TypeResolver resolver = container.Resolve<TypeResolver>();
			resolver.LoadCommands(new string[] { "SignalRHost.Messaging.Commands" });
			resolver.LoadHandlers(new string[] { "SignalRHost.Handlers" });
			container.RegisterInstance<TypeResolver>(resolver);

			container.RegisterType<ChatRouter>();
			container.RegisterType<ChatConnection>();

			// can't get ctor for IPersistenConnectionContext, so we'll use InjectionFactory
			container.RegisterType<IPersistentConnectionContext>(new InjectionFactory(x =>
				unity.Resolve<IConnectionManager>().GetConnectionContext<ChatConnection>()
			));

			// For more information on how to configure your application, visit http://go.microsoft.com/fwlink/?LinkID=316888
			app.Map("/chat", x =>
			{
				x.UseCors(CorsOptions.AllowAll);
				x.RunSignalR<ChatConnection>(new ConnectionConfiguration
				{
					Resolver = unity
				});
			});
		}
	}
}
