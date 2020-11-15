using EventSourcingOnAzureFunctions.Common.CQRS;
using EventSourcingOnAzureFunctions.Common.CQRS.ProjectionHandler.Events;
using EventSourcingOnAzureFunctions.Common.EventSourcing;
using EventSourcingOnAzureFunctions.Common.EventSourcing.Interfaces;
using EventSourcingOnAzureFunctions.Common.Notification;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace EventSourcingOnAzureFunctions.Test
{
    [TestClass]
    public sealed class ProjectionRequested_UnitTest
    {

        [TestMethod]
        public void ToQueueMessage_TestMethod()
        {
            string expected = "|Bank|Account|A-123456-BB|Balance|null|";
            string actual = "";

            ProjectionRequested testObj = new ProjectionRequested()
            {
                ProjectionDomainName = "Bank",
                ProjectionEntityTypeName = "Account",
                ProjectionInstanceKey = "A-123456-BB",
                ProjectionTypeName = "Balance"
            };

            actual = ProjectionRequested.ToQueueMessage(testObj);

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void FromQueueMessage_Blank_TestMethod()
        {
            string testMessage = "";

            var testObj = ProjectionRequested.FromQueuedMessage(testMessage);

            Assert.IsNull(testObj);
        }

        [TestMethod]
        public void FromQueueMessage_Valid_TestMethod()
        {
            string testMessage = "E|Projection Requested|AAAA-BBBB-CDEF012345|2|null||Bank|Account|A-123456-BB|Balance|null|";

            var testObj = ProjectionRequested.FromQueuedMessage(testMessage);

            Assert.IsNotNull(testObj);
        }

        [TestMethod]
        public void FromQueueMessage_Valid_Correlation_TestMethod()
        {

            string expected = "123456789-ab";
            string actual = "not set";

            IEventStreamIdentity  cmdTest = new Common.Binding.EventStreamAttribute("Bank", "Apply Interest", "A-123456-BB");

            string testMessage = QueueNotificationDispatcher.MakeMessageString(cmdTest,
                QueueNotificationDispatcher.NOTIFICATION_NEW_EVENT,
                "Projection Requested",
                3);

            testMessage += $"|Bank|Account|A-123456-BB|Balance|null|123456789-ab";

            var testObj = ProjectionRequested.FromQueuedMessage(testMessage);

            actual = testObj.CorrelationIdentifier;

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void FromQueueMessage_Valid_AsOfDate_TestMethod()
        {

            DateTime expected = new DateTime(2020,12,19);
            DateTime  actual = new DateTime(1984,3,15);

            IEventStreamIdentity cmdTest = new Common.Binding.EventStreamAttribute("Bank", "Apply Interest", "A-123456-BB");

            string testMessage = QueueNotificationDispatcher.MakeMessageString(cmdTest,
                QueueNotificationDispatcher.NOTIFICATION_NEW_EVENT,
                "Projection Requested",
                3);

            testMessage += $"|Bank|Account|A-123456-BB|Balance|2020-12-19|123456789-ab";

            var testObj = ProjectionRequested.FromQueuedMessage(testMessage);

            actual = testObj.AsOfDate.GetValueOrDefault();

            Assert.AreEqual(expected, actual);
        }
    }
}
