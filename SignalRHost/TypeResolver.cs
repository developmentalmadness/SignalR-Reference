using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Tracing;
using Microsoft.Practices.Unity;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;

namespace SignalRHost
{
	/// <summary>
	/// Provides mapping of message types by name to allow message passing from clients
	/// w/o requiring namespaces. 
	/// </summary>
	/// <remarks>
	/// This isn't as clean an abstraction as I'd like. It supports loosly-coupled late
	/// binding in our messages, but it doesn't feel quite right yet.
	/// </remarks>
	public class TypeResolver
	{
		IDictionary<String, Type> commands = new Dictionary<String, Type>();
		IDictionary<Type, Type> handlers = new Dictionary<Type, Type>();

		private static Object sync_lock = new Object();
		private IEnumerable<Type> types;

		private IUnityContainer container;
		private TraceSource logger;

		public TypeResolver(IUnityContainer container)
		{
			this.container = container;

			// FIXME: SignalR's default services are registered privatly w/in DefaultDependencyResolver so they aren't accessable as chained dependencies
			this.logger = container.Resolve<IDependencyResolver>().Resolve<ITraceManager>()["SignalRHost"];
			GetExportedTypes();
		}

		internal IEnumerable<Type> GetExportedTypes()
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

			foreach (var t in list)
			{
				if (commands.ContainsKey(t.Name))
				{
					logger.TraceWarning("Repeat attempt to load command '{0}'", t.FullName);
					continue;
				}

				commands.Add(t.Name, t);
			}
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
			if (handlers.ContainsKey(commandType))
			{
				// define new IHandler<T> type based on commandType parameter
				var hType = handlers[commandType];

				return container.Resolve(hType);
			}

			return null;
		}

		public object ResolveCommandHandler(string commandName)
		{
			var type = FindCommandType(commandName);
			
			if (type != null)
				return ResolveCommandHandler(type);

			return null;
		}

		public Type FindCommandType(string name)
		{
			if (commands.ContainsKey(name))
				return commands[name];

			return null;
		}
	}
}
