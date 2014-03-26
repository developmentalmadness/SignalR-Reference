using Microsoft.AspNet.SignalR;
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
		protected override Task OnConnected(IRequest request, string connectionId)
		{
			return base.OnConnected(request, connectionId);
		}

		
		protected override Task OnReceived(IRequest request, string connectionId, string data)
		{
			var obj = JToken.Parse(data);
			if (obj is JObject)
			{
				JToken type = obj.First()[0];
				// get name as string

				// load .NET type

				// call ToObject(System.Type)

				// resolve Handler<T>(T)
			}
			return base.OnReceived(request, connectionId, data);
		}
	}
}
