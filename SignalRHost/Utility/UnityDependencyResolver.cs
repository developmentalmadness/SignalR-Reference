using Microsoft.AspNet.SignalR;
using Microsoft.Practices.Unity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SignalRHost.Utility
{
	public class UnityDependencyResolver : DefaultDependencyResolver
	{
		private UnityContainer _container;
		public UnityDependencyResolver(UnityContainer container)
		{
			_container = container;
		}
		public override object GetService(Type serviceType)
		{
			if (_container.IsRegistered(serviceType))
			{
				return _container.Resolve(serviceType);
			}
			return base.GetService(serviceType);
		}
		public override IEnumerable<object> GetServices(Type serviceType)
		{
			return _container.ResolveAll(serviceType)
				.Concat(base.GetServices(serviceType));
		}
	}
}
