using EventSourcingOnAzureFunctions.Common;
using EventSourcingOnAzureFunctions.Common.Binding;
using EventSourcingOnAzureFunctions.Common.EventSourcing;
using EventSourcingOnAzureFunctions.Common.Notification;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Mocking;
using System;
using System.Threading.Tasks;

namespace EventSourcingOnAzureFunctions.Test
{
    [TestClass]
    public class EventStream_UnitTest
    {
        //EventStream

        [TestInitialize]
        public void InitialiseEnvironmentVariables()
        {
            DotNetEnv.Env.Load();
        }

        [TestMethod]
        public void Constructor_TestMethod()
        {
            EventStream testObj = new EventStream(new EventStreamAttribute("Bank", "Account", "Instance 1234"));

            Assert.IsNotNull(testObj); 
        }

        [TestMethod]
        public async Task Append_Event_TestMethod()
        {

            bool expected = true;
            bool actual = false;

            EventStream testObj = new EventStream(new EventStreamAttribute("Bank", 
                "Account", "Instance 1234"));

            MockEventOne testEvent = new MockEventOne() { EventTypeName = "Test Event Happened" };

            await testObj.AppendEvent(testEvent);

            actual = await testObj.Exists();

            Assert.AreEqual(expected, actual); 
        }

        [TestMethod]
        public async Task Append_Event_Queue_TestMethod()
        {

            bool expected = true;
            bool actual = false;

            EventStream testObj = new EventStream(new EventStreamAttribute("Bank",
                "Account", 
                "Instance 1234",
                notificationDispatcherName: nameof(QueueNotificationDispatcher ))
                );

            MockEventOne testEvent = new MockEventOne() { EventTypeName = "Test Event Happened" };

            await testObj.AppendEvent(testEvent);

            actual = await testObj.Exists();

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public async Task CreateNew_TestMethod()
        {

            bool expected = true;
            bool actual = false;

            EventStream testObj = new EventStream(new EventStreamAttribute("Bank",
                "Account", Guid.NewGuid().ToString()  ));

            MockEventOne testEvent = new MockEventOne() { EventTypeName = "Another test Event Happened" };

            await testObj.AppendEvent(testEvent);

            actual = await testObj.Exists();

            Assert.AreEqual(expected, actual);
        }
    }
}
