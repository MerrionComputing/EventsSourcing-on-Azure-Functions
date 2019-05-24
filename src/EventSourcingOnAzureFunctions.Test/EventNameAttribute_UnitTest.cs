using EventSourcingOnAzureFunctions.Common.EventSourcing;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace EventSourcingOnAzureFunctions.Test
{
    [TestClass]
    public class EventNameAttribute_UnitTest
    {
        [TestMethod]
        public void Constructor_TestMethod()
        {

            EventNameAttribute testObj = new EventNameAttribute("Test Event Occured");
            Assert.IsNotNull(testObj);

        }

        [TestMethod]
        public void RoundTrip_TestMethod()
        {

            string expected = "Test Method Occurred";
            string actual = "Not set";

            EventNameAttribute testObj = new EventNameAttribute(expected );
            actual = testObj.Name;

            Assert.AreEqual(expected, actual);

        }

        [TestMethod]
        public void AttributedClass_TestMethod()
        {

            string expected = "Mock Test Event";
            string actual = "Not set";

            actual = EventNameAttribute.GetEventName(typeof(EventNameAttribute_Mock ));

            Assert.AreEqual(expected, actual);

        }

    }

    [EventName("Mock Test Event") ]
    public class EventNameAttribute_Mock
    {



    }
}
