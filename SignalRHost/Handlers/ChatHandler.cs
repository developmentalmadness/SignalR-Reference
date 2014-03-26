using Microsoft.AspNet.SignalR;
using SignalRHost.Messaging.Commands;
using SignalRHost.Messaging.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SignalRHost.Handlers
{
	public class ChatHandler : IHandler<Send>
	{
		IConnection bus;
		IConnectionGroupManager groups;

		public ChatHandler(IConnection bus, IConnectionGroupManager groups)
		{
			this.bus = bus;
			this.groups = groups;
		}

		public Task Handle(Send message)
		{
			if (message.Groups.Length == 0)
				message.Groups = new string[] { "All" };

			return groups.Send(new List<string>(message.Groups), new MessageSent
			{
				Username = message.Username,
				Message = message.Message,
				Timestamp = DateTimeOffset.UtcNow
			}, message.ConnectionId);
		}
	}
}
