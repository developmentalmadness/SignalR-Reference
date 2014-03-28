using Microsoft.AspNet.SignalR;
using Microsoft.Practices.Unity;
using System.Threading.Tasks;

namespace SignalRHost
{
	public class ChatConnection : PersistentConnection
	{
		private ChatRouter router;

		public ChatConnection(ChatRouter bus)
		{
			this.router = bus;
		}

		protected override Task OnConnected(IRequest request, string connectionId)
		{
			return router.OnConnected(request, connectionId);
		}
		
		protected override Task OnReceived(IRequest request, string connectionId, string data)
		{
			return router.OnReceived(request, connectionId, data);
		}
	}
}
