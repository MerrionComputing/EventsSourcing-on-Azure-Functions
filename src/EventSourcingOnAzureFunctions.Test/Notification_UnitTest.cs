using EventSourcingOnAzureFunctions.Common.Binding;
using EventSourcingOnAzureFunctions.Common.Listener;
using EventSourcingOnAzureFunctions.Common.Notification;
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


        //Make trace parent
        [TestMethod]
        public void MakeTraceParent_Empty_TestMethod()
        {

            string expected = "00-00000000000000000000000000000000-0000000000000000-00";
            string actual = "Not set";

            actual = EventGridNotificationDispatcher.MakeTraceParent(0,0);

            Assert.AreEqual(expected, actual);
            
        }

        [TestMethod]
        public void MakeTraceParent_NotEmpty_TestMethod()
        {

            string expected = "00-00000000000000000000000000000000-0000000000000000-00";
            string actual = "Not set";

            actual = EventGridNotificationDispatcher.MakeTraceParent(408, 216);

            Assert.AreNotEqual(expected, actual);

        }
    }
}
