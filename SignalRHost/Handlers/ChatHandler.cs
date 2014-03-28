using Microsoft.AspNet.SignalR;
using SignalRHost.Messaging.Commands;
using SignalRHost.Messaging.Events;
using System;
using System.Threading.Tasks;

namespace SignalRHost.Handlers
{
	public class ChatHandler : IHandler<Send>
	{
		IPersistentConnectionContext context;

		public ChatHandler(IPersistentConnectionContext context)
		{
			this.context = context;
		}

		public Task Handle(IRequest request, Send message)
		{
			if (message.Groups.Length == 0)
				message.Groups = new string[] { "All" };

			
			return context.Connection.Broadcast(new MessageSent
				{
					Username = message.Username,
					Message = message.Message,
					Timestamp = DateTimeOffset.UtcNow
				}
			);
		}
	}
}
