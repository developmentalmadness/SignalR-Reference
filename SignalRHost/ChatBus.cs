using Microsoft.AspNet.SignalR;
using System.Threading.Tasks;

namespace SignalRHost
{
	/// <summary>
	/// Threw this class in until I can figure out the best way to test a class that inherits from PersistentConnection
	/// </summary>
	public class ChatBus
	{
		TypeResolver resolver;

		public ChatBus(TypeResolver resolver)
		{
			this.resolver = resolver;
		}

		public Task OnReceived(IRequest request, string connectionId, string data)
		{
			dynamic cmd = resolver.ResolveCommand(data);

			cmd.ConnectionId = connectionId;

			dynamic handler = resolver.ResolveCommandHandler(cmd.GetType());

			if (handler != null)
				return handler.Handle(request, cmd);

			return EmptyTask();
		}

		private Task EmptyTask()
		{
			var tcs = new TaskCompletionSource<object>();
			tcs.SetResult(null);
			return tcs.Task;
		}
	}
}
