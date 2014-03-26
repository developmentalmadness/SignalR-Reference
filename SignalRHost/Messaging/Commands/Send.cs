using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SignalRHost.Messaging.Commands
{
	public class Send
	{
		public string ConnectionId { get; set; }
		public string Username { get; set; }
		public string Message { get; set; }
		public string[] Groups { get; set; }
	}
}
