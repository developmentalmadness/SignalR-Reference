using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SignalRHost
{
	interface IHandler<T> where T : class
	{
		Task Handle(T message);
	}
}
