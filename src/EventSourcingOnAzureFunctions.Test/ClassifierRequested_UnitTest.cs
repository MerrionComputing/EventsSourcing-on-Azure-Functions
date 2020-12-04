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

        [TestMethod]
        public async Task RunClassificationForQuery_TestMethod()
        {

            int expected = 1;
            int actual = 0;

            Query testQuery = new Query("Bank",
                "Get Available Balance",
                "QRY-TEST-A0004"
                );

            // Add a projection-requested event
            await testQuery.RequestClassification("Bank",
                       "Account",
                       "A-001-223456-B",
                       "In Credit",
                       null,
                       null);

            // Perform the projection request
            ClassifierRequestedEventGridEventData clsReq = new ClassifierRequestedEventGridEventData()
            {
                Commentary = "This is a unit test",
                DomainName = testQuery.DomainName,
                EntityTypeName = testQuery.QueryName,
                InstanceKey = testQuery.UniqueIdentifier,
                ClassifierRequest  =
                   new Common.CQRS.ClassifierHandler.Events.ClassifierRequested()
                   {
                       DomainName = "Bank",
                       EntityTypeName = "Account",
                       InstanceKey = "A-001-223456-B",
                       ClassifierTypeName  = "In Credit"
                   }
            };

            await ClassifierHandlerFunctions.RunClassificationForQuery(clsReq);

            // Add an unprocessed request so that we have something to verify was not processed
            await testQuery.RequestClassification("Bank",
                       "Account",
                       "B-001-223456-B",
                       "In Credit",
                       null,
                       null);

            // Now check that there was a classifier response...
            var outstanding = await testQuery.GetOutstandingClassifiers();
            if (outstanding != null)
            {
                actual = outstanding.Count();
            }

            Assert.AreEqual(expected, actual);

        }

    }
}
