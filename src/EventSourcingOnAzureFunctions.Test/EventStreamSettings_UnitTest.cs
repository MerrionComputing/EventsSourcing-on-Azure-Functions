using EventSourcingOnAzureFunctions.Common.Binding;
using EventSourcingOnAzureFunctions.Common.EventSourcing;
using EventSourcingOnAzureFunctions.Common.EventSourcing.Implementation.AzureStorage.Table;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace EventSourcingOnAzureFunctions.Test
{
    [TestClass]
    public class EventStreamSettings_UnitTest
    {

        [TestMethod]
        public void Constructor_TestMethod()
        {

            EventStreamSettings testObj = new EventStreamSettings();

            Assert.IsNotNull(testObj);

        }


        [TestMethod]
        public void LoadFrom_Config_TestMethod()
        {

            string expected = "APPENDBLOB";
            string actual = "Not set";

            EventStreamSettings testObj = new EventStreamSettings();
            testObj.LoadFromConfig();

            actual = testObj.GetBackingImplementationType(new EventStreamAttribute("Domain Test", "Entity Type Test Two", "Instance 123"));

            Assert.AreEqual(expected, actual);  
        }

        [TestMethod]
        public void ImplementationType_Table__TestMethod()
        {

            string expected = "TABLE";
            string actual = "Not set";

            EventStreamSettings testObj = new EventStreamSettings();
            testObj.LoadFromConfig();

            actual = testObj.GetBackingImplementationType(new EventStreamAttribute("Domain Test", "Entity Type Test", "Instance 123"));

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void CreateWriter_Table__TestMethod()
        {


            EventStreamSettings testObj = new EventStreamSettings();
            testObj.LoadFromConfig();

            var writerObj = testObj.CreateWriterForEventStream(new EventStreamAttribute("Domain Test", "Entity Type Test", "Instance 123"));

            Assert.IsInstanceOfType(writerObj, typeof(TableEventStreamWriter));
        }

    }
}
