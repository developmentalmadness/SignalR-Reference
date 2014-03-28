using Microsoft.AspNet.SignalR;
using Microsoft.Practices.Unity;
using System.Threading.Tasks;

namespace SignalRHost
{
	public class ChatConnection : PersistentConnection
	{
		private ChatBus bus;

		public ChatConnection(ChatBus bus)
		{
			this.bus = bus;
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
