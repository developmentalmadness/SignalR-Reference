using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SignalRHost;
using Microsoft.AspNet.SignalR;
using Moq;
using SignalRHost.Messaging.Commands;
using SignalRHost.Handlers;

namespace ChatApp.Test
{
	[TestClass]
	public class UnitTest1
	{
		Mock<IConnection> bus;
		Mock<IConnectionGroupManager> groups;
		Mock<IDependencyResolver> resolver;

		[TestInitialize]
		public void Init() 
		{
			bus = new Mock<IConnection>(MockBehavior.Strict);
			groups = new Mock<IConnectionGroupManager>(MockBehavior.Strict);
			resolver = new Mock<IDependencyResolver>(MockBehavior.Strict);
		}

		[TestCleanup]
		public void Cleanup()
		{
			bus.VerifyAll();
			groups.VerifyAll();
			resolver.VerifyAll();
		}

		[TestMethod]
		public void TestMethod1()
		{
			var request = new Mock<IRequest>();

			var resolver = new TypeResolver(this.resolver.Object);
			resolver.LoadCommands(new string[] { "SignalRHost.Messaging.Commands" });
			var target = new ChatBus(resolver);
			var task = target.OnReceived(request.Object, Guid.NewGuid().ToString(), "{ \"Send\": { \"Username\": \"Mark\", \"Message\": \"Hello, World!\", \"Groups\": [\"All\"] } }");

			task.RunSynchronously();

		}

		[TestMethod]
		public void LoadCommands()
		{
			var target = new TypeResolver(resolver.Object);
			target.LoadCommands(new string[] { "SignalRHost.Messaging.Commands" });
			var actual = target.FindCommandType("Send");

			Assert.AreEqual(typeof(Send), actual);
		}

		[TestMethod]
		public void LoadHandlers()
		{
			var bus = new Mock<IConnection>(MockBehavior.Strict);
			var groups = new Mock<IConnectionGroupManager>(MockBehavior.Strict);

			resolver.Setup(r => r.GetService(It.IsAny<Type>()))
				.Returns(new ChatHandler(bus.Object, groups.Object));

			var target = new TypeResolver(resolver.Object);
			target.LoadHandlers(new string[] { "SignalRHost.Handlers" });
			var actual = target.FindHandlerType("ChatHandler");

			Assert.AreEqual(typeof(ChatHandler), actual.GetType());
		}
	}
}
