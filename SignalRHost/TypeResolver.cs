using Microsoft.Practices.Unity;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace SignalRHost
{
	public class TypeResolver
	{
		IEnumerable<Type> commands;
		IDictionary<Type, Type> handlers = new Dictionary<Type, Type>();

		private static object sync_lock = new object();
		private static IEnumerable<Type> types;

		public IUnityContainer container;

		public TypeResolver(IUnityContainer container)
		{
			this.container = container;
			GetExportedTypes();
		}

		internal static IEnumerable<Type> GetExportedTypes()
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

			foreach (var h in list)
			{
				var interfaces = from i in h.GetInterfaces()
								 where i.IsGenericType
									&& typeof(IHandler<>).IsAssignableFrom(i.GetGenericTypeDefinition())
									select i;

				lock (sync_lock)
				{
					foreach (var i in interfaces)
					{
						container.RegisterType(i, h);
						handlers.Add(
							i.GetGenericArguments()[0],
							i
						);
					}
				}
			}
		}

		public object ResolveCommand(string data)
		{
			var obj = JToken.Parse(data) as JObject;

			if (obj != null)
			{
				var type = obj.First as JProperty;

				if (type != null)
				{
					// get name as string
					var name = type.Name;

					// load .NET type
					var t = FindCommandType(name);

					// call ToObject(System.Type)
					var value = type.Value as JObject;

					if (value != null)
						return value.ToObject(t);
				}
			}

			return null;
		}

		public object ResolveCommandHandler(Type commandType)
		{
			// define new IHandler<T> type based on commandType parameter
			var hType = handlers[commandType];

			return container.Resolve(hType);
		}

		public Type FindCommandType(string name)
		{
			return commands.SingleOrDefault(c => c.Name == name);
		}
	}
}
