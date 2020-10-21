using EventSourcingOnAzureFunctions.Common.Binding;
using EventSourcingOnAzureFunctions.Common.CQRS;
using EventSourcingOnAzureFunctions.Common.CQRS.ClassifierHandler.Functions;
using EventSourcingOnAzureFunctions.Common.EventSourcing;
using EventSourcingOnAzureFunctions.Common.EventSourcing.Implementation;
using EventSourcingOnAzureFunctions.Common.EventSourcing.Interfaces;
using EventSourcingOnAzureFunctions.Common.Notification;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Threading.Tasks;

namespace EventSourcingOnAzureFunctions.Test
{
    [TestClass]
    public class QueueNotificationDispatcher_UnitTest
    {

        [TestMethod]
        public void MakeQueueName_Valid_TestMethod()
        {
            string expected = "bank-account";
            string actual = "not set";

            IEventStreamIdentity target = new EventStreamAttribute("Bank", "Account", "J920377-A");
            actual = QueueNotificationDispatcher.MakeQueueName(target); 

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void MakeQueueName_Inalid_TestMethod()
        {
            string expected = "bank-account";
            string actual = "not set";

            IEventStreamIdentity target = new EventStreamAttribute("1Bank", "Account-", "J920377-A");
            actual = QueueNotificationDispatcher.MakeQueueName(target);

            Assert.AreEqual(expected, actual);
        }

    }
}
