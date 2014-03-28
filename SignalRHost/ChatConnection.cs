using Microsoft.AspNet.SignalR;
using Microsoft.Practices.Unity;
using System.Threading.Tasks;

namespace SignalRHost
{
	public class ChatConnection : PersistentConnection
	{
		private ChatBus bus;
		private IUnityContainer container;

		public ChatConnection()
		{
			// TODO: figure out how to get IoC to work with a PersistentConnection class. Symptom is that the client can't connect
			container = GlobalHost.DependencyResolver.Resolve<IUnityContainer>();
			bus = container.Resolve<ChatBus>();
		}

		protected override Task OnConnected(IRequest request, string connectionId)
		{
			//Groups.Add(connectionId, "All");
			return base.Connection.Broadcast("Welcome!");
		}

		
		protected override Task OnReceived(IRequest request, string connectionId, string data)
		{
			return bus.OnReceived(request, connectionId, data);
		}
	}
}
