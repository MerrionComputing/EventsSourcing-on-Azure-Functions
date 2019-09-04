using EventSourcingOnAzureFunctions.Common.Binding;
using EventSourcingOnAzureFunctions.Common.Listener;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace EventSourcingOnAzureFunctions.Test
{
    [TestClass]
    public class Notification_UnitTest
    {

        [TestMethod]
        public void Constructor_NewEntity_TestMethod()
        {

            Notification testObj = Notification.NewEntityNotification(new EventStreamAttribute("Domain Test", "Entity Type Test Two", "Instance 123"));

            Assert.IsNotNull(testObj);

        }

        [TestMethod]
        public void Constructor_NewEvent_TestMethod()
        {

            Notification testObj = Notification.NewEventNotification(new EventStreamAttribute("Domain Test", "Entity Type Test Two", "Instance 123"), "Test Event Occured");

            Assert.IsNotNull(testObj);

        }

        [TestMethod]
        public void MatchesFilter_True_NewEvent_TestMethod()
        {
            bool expected = true;
            bool actual = false;

            Notification testObj = Notification.NewEventNotification(new EventStreamAttribute("Domain Test", "Entity Type Test Two", "Instance 123"), "Test Event Occured");

            actual =  testObj.MatchesFilter(  Notification.NotificationType.NewEvent, "Domain Test", "Entity Type Test Two", "Instance 123", "Test Event Occured");

            Assert.AreEqual(expected, actual); 
        }

        [TestMethod]
        public void MatchesFilter_True_NewEntity_TestMethod()
        {
            bool expected = true;
            bool actual = false;

            Notification testObj = Notification.NewEntityNotification(new EventStreamAttribute("Domain Test", "Entity Type Test Two", "Instance 123"));

            actual = testObj.MatchesFilter(Notification.NotificationType.NewEntity, "Domain Test", "Entity Type Test Two", "Instance 123", "Test Event Occured");

            Assert.AreEqual(expected, actual);
        }
    }
}
