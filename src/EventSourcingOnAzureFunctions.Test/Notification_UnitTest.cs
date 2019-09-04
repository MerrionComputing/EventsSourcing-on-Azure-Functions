using EventSourcingOnAzureFunctions.Common.Binding;
using EventSourcingOnAzureFunctions.Common.Listener;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace EventSourcingOnAzureFunctions.Test
{
    [TestClass]
    public class Notification_UnitTest
    {

        [TestMethod]
        public void Constructor_TestMethod()
        {

            Notification testObj = Notification.NewEntityNotification(new EventStreamAttribute("Domain Test", "Entity Type Test Two", "Instance 123"));

            Assert.IsNotNull(testObj);

        }

    }
}
