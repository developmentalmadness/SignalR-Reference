using Microsoft.AspNet.SignalR;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace SignalRHost
{
	public class TypeResolver
	{
		IEnumerable<Type> commands;
		IEnumerable<Type> handlers;

		private static object sync_lock = new object();
		private static IEnumerable<Type> types;

		public IDependencyResolver container;

		public TypeResolver(IDependencyResolver container)
		{
			this.container = container;	
		}

		private static IEnumerable<Type> GetExportedTypes()
		{
			if (types == null)
			{
				lock (sync_lock)
				{
					if (types == null)
					{
						var list = new List<Assembly>();

						list.Add(Assembly.Load("SignalRHost, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null"));
						
						types = (from assembly in list
								where assembly.IsDynamic == false
								from t in assembly.GetExportedTypes()
								select t).ToArray();
					}
				}
			}

			return types;
		}

		public void LoadCommands(string[] namespaces)
		{
			IEnumerable<Type> list = null;

			list = from t in GetExportedTypes()
				   where namespaces.Any(n => n == t.Namespace)
				   select t;

			if (commands == null)
				commands = list;
			else
				commands = commands.Union(list);
		}

		public void LoadHandlers(string[] namespaces)
		{
			IEnumerable<Type> list = null;

			list = from t in GetExportedTypes()
				   where namespaces.Any(n => n == t.Namespace)
				   from i in t.GetInterfaces()
				   where i.IsGenericType
					&& typeof(IHandler<>).IsAssignableFrom(i.GetGenericTypeDefinition())
				   select t;

			container.Register()

			if (handlers == null)
				handlers = list;
			else
				handlers = handlers.Union(list);
		}

		public object ResolveCommand(string data)
		{
			var obj = JToken.Parse(data);

			if (obj is JObject)
			{
				JToken type = obj.First();
				if (type == null) return null;

				// get name as string
				var name = ((JProperty)type).Name;

				// load .NET type
				var t = FindCommandType(name);

				// call ToObject(System.Type)
				return obj.ToObject(t);
			}

			return null;
		}

		public object ResolveCommandHandler(Type commandType)
		{
			return (
					from h in handlers
						from i in h.GetInterfaces()
							where i.IsGenericType
								&& commandType.IsAssignableFrom(i.GetGenericArguments()[0])
								&& typeof(IHandler<>).IsAssignableFrom(i.GetGenericTypeDefinition())
					select h
				).SingleOrDefault();
		}

		public Type FindCommandType(string name)
		{
			return commands.SingleOrDefault(c => c.Name == name);
		}

		public object FindHandlerType(string name)
		{
			var handler = handlers.SingleOrDefault(h => h.Name == name);
			if (handler == null)
				return null;

			return container.Resolve(handler);
		}
	}
}
