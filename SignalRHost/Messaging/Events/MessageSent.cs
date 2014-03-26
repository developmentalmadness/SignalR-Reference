﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SignalRHost.Messaging.Events
{
	public class MessageSent
	{
		public string Username { get; set; }
		public string Message { get; set; }
		public DateTimeOffset Timestamp { get; set; }
	}
}
