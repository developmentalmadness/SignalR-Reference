using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Tracing;
using Microsoft.Practices.Unity;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Newtonsoft.Json.Linq;
using SignalRHost;
using SignalRHost.Messaging.Commands;
using SignalRHost.Messaging.Events;
using SignalRHost.Utility;
using System;
using System.Diagnostics;

namespace ChatApp.Test
{
	[TestClass]
	public class UnitTest1
	{
		Mock<IConnection> connection;
		Mock<IConnectionGroupManager> groups;
		Mock<IPersistentConnectionContext> context;
		Mock<ITraceManager> traceManager;

		TypeResolver resolver;
		IUnityContainer container;

		[TestInitialize]
		public void Init() 
		{
			connection = new Mock<IConnection>(MockBehavior.Strict);
			groups = new Mock<IConnectionGroupManager>(MockBehavior.Strict);
			context = new Mock<IPersistentConnectionContext>(MockBehavior.Strict);
			traceManager = new Mock<ITraceManager>(MockBehavior.Strict);

			context.SetupGet(x => x.Connection).Returns(connection.Object);
			context.SetupGet(x => x.Groups).Returns(groups.Object);

			traceManager.SetupGet(x => x["SignalRHost"]).Returns(new TraceSource("SignalRHost"));

			container = new UnityContainer();

			container.RegisterInstance<IConnection>(connection.Object);
			container.RegisterInstance<IConnectionGroupManager>(groups.Object);
			container.RegisterInstance<IPersistentConnectionContext>(context.Object);
			container.RegisterInstance<ITraceManager>(traceManager.Object);

			resolver = container.Resolve<TypeResolver>();
			resolver.LoadCommands(new string[] { "SignalRHost.Messaging.Commands" });
			resolver.LoadHandlers(new string[] { "SignalRHost.Handlers" });
			container.RegisterInstance<TypeResolver>(resolver);
		}

		private void SetupSend(Func<ConnectionMessage, bool> verify)
		{
			connection.SetupGet(x => x.DefaultSignal).Returns("toutlemonde");
			connection.Setup(x => x.Send(It.Is<ConnectionMessage>(m => verify.Invoke(m)))).Returns(TaskAsyncHelper.Empty);
		}

		[TestCleanup]
		public void Cleanup()
		{
			connection.VerifyAll();
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

			SetupSend(x =>
					x.Value is MessageSent
					&& ((MessageSent)x.Value).Message == "Hello, World!"
					&& ((MessageSent)x.Value).Username == "Mark"
					&& ((MessageSent)x.Value).Timestamp >= start
				);

			var request = new Mock<IRequest>();

			var target = new ChatRouter(resolver);
			var task = target.OnReceived(request.Object, connectionId, "{ \"Send\": { \"Username\": \"Mark\", \"Message\": \"Hello, World!\", \"Groups\": [\"All\"] } }");

			if (!task.IsCompleted)
			{
				task.Start();
				task.Wait(TimeSpan.FromSeconds(5));
			}
		}

		[TestMethod]
		public void LoadCommands()
		{
			var actual = resolver.FindCommandType("Send");

			Assert.AreEqual(typeof(Send), actual);
		}
	}
}
