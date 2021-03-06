﻿using Microsoft.AspNet.SignalR;
using SignalRHost.Utility;
using System.Threading.Tasks;

namespace SignalRHost
{
	/// <summary>
	/// Routes commands to their mapped command handler
	/// </summary>
	/// <remarks>
	/// The purpose of this class is two-fold:
	/// <ol>
	///		<li>Provide a testible proxy for our PersistentConnection (ChatConnection) class.</li>
	///		<li>Map JSON messages to POCOs and then to their mapped IHandler&lt;&gt;</li>
	/// </ol>
	/// </remarks>
	public class ChatRouter
	{
		TypeResolver resolver;
		IPersistentConnectionContext context;

		public ChatRouter(TypeResolver resolver, IPersistentConnectionContext context)
		{
			this.resolver = resolver;
			this.context = context;
		}

		public Task OnConnected(IRequest request, string connectionId)
		{
			return context.Connection.Broadcast("Welcome!");
		}

		public Task OnReceived(IRequest request, string connectionId, string data)
		{
			dynamic cmd = resolver.ResolveCommand(data);

			cmd.ConnectionId = connectionId;

			dynamic handler = resolver.ResolveCommandHandler(cmd.GetType());

			if (handler != null)
				return handler.Handle(request, cmd);

			return TaskAsyncHelper.Empty;
		}
	}
}
