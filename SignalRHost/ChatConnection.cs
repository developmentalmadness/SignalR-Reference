using Microsoft.AspNet.SignalR;
using Microsoft.Practices.Unity;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SignalRHost.Messaging.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SignalRHost
{
	public class ChatConnection : PersistentConnection
	{
		private ChatBus bus;
		private IUnityContainer container;

		public ChatConnection()
		{
			container = GlobalHost.DependencyResolver.Resolve<IUnityContainer>();
			bus = container.Resolve<ChatBus>();
		}

		protected override async Task OnConnected(IRequest request, string connectionId)
		{
			//Groups.Add(connectionId, "All");
			await base.Connection.Broadcast("Welcome!");
			//return base.OnConnected(request, connectionId);
		}

		
		protected override Task OnReceived(IRequest request, string connectionId, string data)
		{
			//return base.OnReceived(request, connectionId, data);
			using (var child = container.CreateChildContainer())
			{
				child.RegisterInstance<IConnection>(this.Connection);
				child.RegisterInstance<IConnectionGroupManager>(this.Groups);
				return bus.OnReceived(request, connectionId, data);
			}
		}
	}
}
