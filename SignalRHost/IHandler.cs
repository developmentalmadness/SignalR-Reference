using Microsoft.AspNet.SignalR;
using System.Threading.Tasks;

namespace SignalRHost
{
	interface IHandler<T> where T : class
	{
		Task Handle(IRequest request, T message);
	}
}
