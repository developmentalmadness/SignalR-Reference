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
				var cmd = resolver.ResolveCommand(data);

				dynamic handler = resolver.ResolveCommandHandler(cmd.GetType());
				if(handler != null)
					handler.Handle(cmd);
			});
		}
	}
}
