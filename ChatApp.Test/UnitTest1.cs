using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SignalRHost;
using Microsoft.AspNet.SignalR;
using Moq;
using SignalRHost.Messaging.Commands;
using SignalRHost.Handlers;
using Microsoft.Practices.Unity;
using System.Collections.Generic;
using SignalRHost.Messaging.Events;
using Newtonsoft.Json.Linq;

namespace ChatApp.Test
{
	[TestClass]
	public class UnitTest1
	{
		Mock<IConnection> bus;
		Mock<IConnectionGroupManager> groups;

		TypeResolver resolver;
		IUnityContainer container;

		[ClassInitialize]
		public static void Startup(TestContext ctx)
		{
			TypeResolver.GetExportedTypes();
		}

		[TestInitialize]
		public void Init() 
		{
			bus = new Mock<IConnection>(MockBehavior.Strict);
			groups = new Mock<IConnectionGroupManager>(MockBehavior.Strict);

			container = new UnityContainer();
			resolver = new TypeResolver(container);

			container.RegisterInstance<IConnection>(bus.Object);
			container.RegisterInstance<IConnectionGroupManager>(groups.Object);

			resolver.LoadCommands(new string[] { "SignalRHost.Messaging.Commands" });
			resolver.LoadHandlers(new string[] { "SignalRHost.Handlers" });
		}

		[TestCleanup]
		public void Cleanup()
		{
			bus.VerifyAll();
			groups.VerifyAll();
		}

		/// <summary>
		/// Just to get an idea of how long it takes to parse a Json object
		/// </summary>
		[TestMethod]
		public void JsonPerfCompare()
		{
			var data = "{ \"Send\": { \"Username\": \"Mark\", \"Message\": \"Hello, World!\", \"Groups\": [\"All\"] } }";
			JToken.Parse(data);
		}

		[TestMethod]
		public void ReceiveCommand()
		{
			var start = DateTimeOffset.Now;
			var connectionId = Guid.NewGuid().ToString();

			groups.Setup(g => g.Send(
				It.Is<List<String>>(x => x.Count == 1 && x[0] == "All"),
				It.Is<object>(x => 
					x is MessageSent 
					&& ((MessageSent)x).Message == "Hello, World!"
					&& ((MessageSent)x).Username == "Mark"
					&& ((MessageSent)x).Timestamp >= start
				),
				It.Is<string[]>(x => x.Length == 1 && x[0] == connectionId)
			));

			var request = new Mock<IRequest>();

			var target = new ChatBus(resolver);
			var task = target.OnReceived(request.Object, connectionId, "{ \"Send\": { \"Username\": \"Mark\", \"Message\": \"Hello, World!\", \"Groups\": [\"All\"] } }");

			task.RunSynchronously();
		}

		[TestMethod]
		public void LoadCommands()
		{
			var actual = resolver.FindCommandType("Send");

			Assert.AreEqual(typeof(Send), actual);
		}
	}
}
