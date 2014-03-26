using Microsoft.AspNet.SignalR;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SignalRHost.Handlers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
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
			return new Task(() =>
			{
				dynamic cmd = resolver.ResolveCommand(data);
				cmd.ConnectionId = connectionId;

				// this sucks - need to be able to pass in arguments or access request context
				dynamic handler = resolver.ResolveCommandHandler(cmd.GetType());
				if(handler != null)
					handler.Handle(cmd);
			});
		}
	}
}
