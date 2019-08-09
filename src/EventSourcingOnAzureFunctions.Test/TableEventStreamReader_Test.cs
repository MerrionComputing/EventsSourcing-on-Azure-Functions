using EventSourcingOnAzureFunctions.Common.Binding;
using EventSourcingOnAzureFunctions.Common.EventSourcing;
using EventSourcingOnAzureFunctions.Common.EventSourcing.Implementation;
using EventSourcingOnAzureFunctions.Common.EventSourcing.Implementation.AzureStorage.Table;
using EventSourcingOnAzureFunctions.Common.EventSourcing.Interfaces;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Mocking;
using System;
using System.Threading.Tasks;

namespace EventSourcingOnAzureFunctions.Test
{
    [TestClass]
    public class TableEventStreamReader_UnitTest
    {

        [TestMethod]
        public void Constructor_TestMethod()
        {
            TableEventStreamReader testObj = new TableEventStreamReader(
                new EventStreamAttribute("Domain Test", "Entity Type Test", "Instance 123"),
                "RetailBank");

            Assert.IsNotNull(testObj);

        }


        [TestMethod]
        public void Projection_Constructor_TestMethod()
        {

            TableEventStreamReader testReader = new TableEventStreamReader(
                new EventStreamAttribute("Domain Test", "Entity Type Test", "Instance 123"),
                "RetailBank");

            ProjectionProcessor testObj = new ProjectionProcessor(testReader);

            Assert.IsNotNull(testReader); 

        }

        [TestMethod]
        public async Task Projection_Exists_TestMethod()
        {
            bool expected = true;
            bool actual = false;

            TableEventStreamReader testReader = new TableEventStreamReader(
                new EventStreamAttribute("Domain Test", "Entity Type Test", "Instance 123"),
                "RetailBank");

            ProjectionProcessor testObj = new ProjectionProcessor(testReader);

            actual = await testObj.Exists();

            Assert.AreEqual(expected, actual);

        }

        [TestMethod]
        public async Task Projection_NotExists_TestMethod()
        {
            bool expected = false ;
            bool actual = true ;

            TableEventStreamReader testReader = new TableEventStreamReader(
                new EventStreamAttribute("Domain Test", "Entity Type Test", "Instance not existing abc.def"),
                "RetailBank");

            ProjectionProcessor testObj = new ProjectionProcessor(testReader);

            actual = await testObj.Exists();

            Assert.AreEqual(expected, actual);

        }

        [TestMethod]
        public void Projection_MockProjectionOne_TestMethod()
        {

            int notexpected = 0;
            int actual = 0;

            TableEventStreamReader testReader = new TableEventStreamReader(
                new EventStreamAttribute("Domain Test", "Entity Type Test", "Instance 123"),
                "RetailBank");

            ProjectionProcessor testObj = new ProjectionProcessor(testReader);

            var result =  testObj.Process<MockProjectionOne>() ;
            actual = result.Result.TotalCount;

            Assert.AreNotEqual (notexpected, actual);

        }

    }
}

namespace Mocking
{

    public class MockProjectionOne
        : ProjectionBase,
        IHandleEventType<MockEventOnePayload>
    {

        private string lastMessage;
        private int intCount;

        public void HandleEventInstance(MockEventOnePayload eventInstance)
        {
            if (null != eventInstance)
            {
                lastMessage = eventInstance.StringProperty;
                intCount += eventInstance.IntegerProperty;
            }
        }

        public int TotalCount
        {
            get
            {
                return intCount;
            }
        }
    }

}
