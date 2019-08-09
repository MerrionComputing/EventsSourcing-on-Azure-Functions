using EventSourcingOnAzureFunctions.Common.EventSourcing;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace EventSourcingOnAzureFunctions.Test
{
    [TestClass ]
    public class EventMaps_UnitTest
    {

        [TestMethod]
        public void Constructor_TestMethod()
        {

            EventMaps testObj = new EventMaps();

            Assert.IsNotNull(testObj);  

        }

        [TestMethod]
        public void LoadFrom_Config_TestMethod()
        {

            EventMaps testObj = new EventMaps();
            testObj.LoadFromConfig();

            var evtClass = testObj.CreateEventClass("Mock Event One");
            Assert.IsNotNull(evtClass); 
            
        }

        [TestMethod]
        public void LoadFrom_Reflection_TestMethod()
        {

            EventMaps testObj = new EventMaps();
            testObj.LoadByReflection();

            var evtClass = testObj.CreateEventClass("Mock Event One");
            Assert.IsNotNull(evtClass);

        }
    }
}
