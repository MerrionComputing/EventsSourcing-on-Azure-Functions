using EventSourcingOnAzureFunctions.Common.CQRS.ClassifierHandler.Events;
using EventSourcingOnAzureFunctions.Common.CQRS;
using EventSourcingOnAzureFunctions.Common.EventSourcing;
using EventSourcingOnAzureFunctions.Common.EventSourcing.Interfaces;
using EventSourcingOnAzureFunctions.Common.Notification;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading.Tasks;
using EventSourcingOnAzureFunctions.Common.CQRS.ClassifierHandler.Functions;
using System.Linq;

namespace EventSourcingOnAzureFunctions.Test
{
    [TestClass]
    public sealed class ClassifierRequested_UnitTest
    {

        [TestMethod]
        public void ToQueueMessage_TestMethod()
        {
            string expected = "|Bank|Account|A-123456-BB|Is Overdrawn|null|";
            string actual = "";

            ClassifierRequested testObj = new ClassifierRequested()
            {
                DomainName = "Bank",
                EntityTypeName = "Account",
                InstanceKey  = "A-123456-BB",
                ClassifierTypeName  = "Is Overdrawn"
            };

            actual = ClassifierRequested.ToQueueMessage(testObj);

            Assert.AreEqual(expected, actual);
        }


        [TestMethod]
        public void FromQueueMessage_Blank_TestMethod()
        {
            string testMessage = "";

            var testObj = ClassifierRequested.FromQueuedMessage(testMessage);

            Assert.IsNull(testObj);
        }

        [TestMethod]
        public void FromQueueMessage_Valid_Correlation_TestMethod()
        {

            string expected = "123456789-ab";
            string actual = "not set";

            IEventStreamIdentity cmdTest = new Common.Binding.EventStreamAttribute("Bank", "Apply Interest", "A-123456-BB");

            string testMessage = QueueNotificationDispatcher.MakeMessageString(cmdTest,
                QueueNotificationDispatcher.NOTIFICATION_NEW_EVENT,
                "Classification Requested",
                8);

            testMessage += $"|Bank|Account|A-123456-BB|Is Overdrawn|null|123456789-ab";

            var testObj = ClassifierRequested.FromQueuedMessage(testMessage);

            actual = testObj.CorrelationIdentifier;

            Assert.AreEqual(expected, actual);
        }


    }
}
