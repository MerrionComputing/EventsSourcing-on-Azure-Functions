using EventSourcingOnAzureFunctions.Common.Binding;
using EventSourcingOnAzureFunctions.Common.EventSourcing;
using EventSourcingOnAzureFunctions.Common.EventSourcing.Implementation.AzureStorage.Table;
using EventSourcingOnAzureFunctions.Common.EventSourcing.Interfaces;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Mocking;
using System;

namespace EventSourcingOnAzureFunctions.Test
{
    [TestClass]
    public class TableEventStreamBase_UnitTest
    {

        [TestMethod]
        public void MakeValidStorageFolderName_TooShort_TestMethod()
        {
            string expected = "aDATA";
            string actual = "";

            actual = TableEventStreamBase.MakeValidStorageTableName("-a");

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void MakeValidStorageTableName_TooShortAfterInvalid_TestMethod()
        {
            string expected = "aDATA";
            string actual = "";

            actual = TableEventStreamBase.MakeValidStorageTableName("- - - - - - - -a");

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void MakeValidStorageTableName_FixChars_TestMethod()
        {
            string expected = "DuncansModel";
            string actual = "";

            actual = TableEventStreamBase.MakeValidStorageTableName("Duncan's Model");

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void SequenceNumberAsString_Zero_TestMethod()
        {


            string expected = "0000000000";
            string actual = "0123456789";

            actual = TableEventStreamBase.SequenceNumberAsString(0);

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void SequenceNumberAsString_NineteenSeventy_TestMethod()
        {


            string expected = "0000001970";
            string actual = "0123456789";

            actual = TableEventStreamBase.SequenceNumberAsString(1970);

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void IsPropertyEmpty_EmptyObject_TestMethod()
        {
            bool expected = true;
            bool actual = false;

            MockEventOnePayload testObj = new MockEventOnePayload();

            actual = TableEventStreamBase.IsPropertyEmpty(typeof(MockEventOnePayload).GetProperty(nameof(MockEventOnePayload.objectProperty)), testObj);

            Assert.AreEqual(expected, actual);

        }

        [TestMethod]
        public void IsPropertyEmpty_EmptyDate_TestMethod()
        {
            bool expected = true;
            bool actual = false;

            MockEventOnePayload testObj = new MockEventOnePayload();

            actual = TableEventStreamBase.IsPropertyEmpty(typeof(MockEventOnePayload).GetProperty(nameof(MockEventOnePayload.dateTimeProperty)), testObj);

            Assert.AreEqual(expected, actual);

        }

        [TestMethod]
        public void IsPropertyEmpty_EmptyDateTimeOffset_TestMethod()
        {
            bool expected = true;
            bool actual = false;

            MockEventOnePayload testObj = new MockEventOnePayload();

            actual = TableEventStreamBase.IsPropertyEmpty(typeof(MockEventOnePayload).GetProperty(nameof(MockEventOnePayload.DateTimeOffsetProperty)), testObj);

            Assert.AreEqual(expected, actual);

        }

        [TestMethod]
        public void IsPropertyEmpty_EarlyDate_TestMethod()
        {
            bool expected = true;
            bool actual = false;

            MockEventOnePayload testObj = new MockEventOnePayload();
            testObj.dateTimeProperty = new DateTime(1290, 1, 1);

            actual = TableEventStreamBase.IsPropertyEmpty(typeof(MockEventOnePayload).GetProperty(nameof(MockEventOnePayload.dateTimeProperty)), testObj);

            Assert.AreEqual(expected, actual);

        }

        [TestMethod]
        public void IsPropertyEmpty_EarlyDateTimeOffset_TestMethod()
        {
            bool expected = true;
            bool actual = false;

            MockEventOnePayload testObj = new MockEventOnePayload();
            testObj.DateTimeOffsetProperty = new DateTimeOffset(new DateTime(1588, 12, 1));
            actual = TableEventStreamBase.IsPropertyEmpty(typeof(MockEventOnePayload).GetProperty(nameof(MockEventOnePayload.DateTimeOffsetProperty)), testObj);

            Assert.AreEqual(expected, actual);

        }

        [TestMethod]
        public void IsPropertyEmpty_LaterDateTimeOffset_TestMethod()
        {
            bool expected = false ;
            bool actual = true;

            MockEventOnePayload testObj = new MockEventOnePayload();
            testObj.DateTimeOffsetProperty = new DateTimeOffset(new DateTime(1988, 12, 1));
            actual = TableEventStreamBase.IsPropertyEmpty(typeof(MockEventOnePayload).GetProperty(nameof(MockEventOnePayload.DateTimeOffsetProperty)), testObj);

            Assert.AreEqual(expected, actual);

        }

        [TestMethod]
        public void MakeDynamicTableEntity_Valid_TestMethod()
        {

            MockEventOne testEvent = new MockEventOne() { EventTypeName = "Tested Event" };
            testEvent.EventPayload = new MockEventOnePayload() { StringProperty = "This is a test", dateTimeProperty = DateTime.Now };

            TableEventStreamWriter tsw = new TableEventStreamWriter(new EventStreamAttribute("Domain Test", "Entity Type Test", "Instance 123"), 
                "RetailBank");

            var testObj = tsw.MakeDynamicTableEntity(testEvent, 123);

            Assert.IsNotNull(testObj); 

        }
    }

}

namespace Mocking
{
    public class MockEventOnePayload
    {
        public int IntegerProperty { get; set; }

        public string StringProperty { get; set; }

        public DateTime dateTimeProperty { get; set; }

        public object objectProperty { get; set; }

        public DateTimeOffset DateTimeOffsetProperty { get; set; }
    }

    public class MockEventOne
        : IEvent
    {



        public string EventTypeName { get; set; }

        public object EventPayload { get; set; }
    }
}
