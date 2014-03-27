using System;
using System.Threading.Tasks;
using Microsoft.Owin;
using Owin;
using Microsoft.Owin.Cors;
using Microsoft.AspNet.SignalR;
using Microsoft.Practices.Unity;
using SignalRHost.Utility;
using System.Reflection;

[assembly: OwinStartup(typeof(SignalRHost.Startup))]

namespace SignalRHost
{
	public class Startup
	{
		public void Configuration(IAppBuilder app)
		{
			var container = new UnityContainer();

			container.RegisterTypes(
				AllClasses.FromAssemblies(Assembly.Load("SignalRHost, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null")),
				WithMappings.FromMatchingInterface,
				WithName.Default
			);

			TypeResolver resolver = container.Resolve<TypeResolver>();
			resolver.LoadCommands(new string[] { "SignalRHost.Messaging.Commands" });
			resolver.LoadHandlers(new string[] { "SignalRHost.Handlers" });
			container.RegisterInstance(resolver);

			// For more information on how to configure your application, visit http://go.microsoft.com/fwlink/?LinkID=316888
			app.Map("/chat", x =>
			{
				x.UseCors(CorsOptions.AllowAll);
				x.RunSignalR<ChatConnection>(new ConnectionConfiguration
				{
					Resolver = new UnityDependencyResolver(container)
				});
			});
		}
	}
}
