using EventSourcingOnAzureFunctions.Common.EventSourcing;
using EventSourcingOnAzureFunctions.Common.EventSourcing.Implementation.AzureStorage.Table;
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

            MockEventOne testObj = new MockEventOne();

            actual = TableEventStreamBase.IsPropertyEmpty(typeof(MockEventOne).GetProperty(nameof(MockEventOne.objectProperty)), testObj);

            Assert.AreEqual(expected, actual);

        }

        [TestMethod]
        public void IsPropertyEmpty_EmptyDate_TestMethod()
        {
            bool expected = true;
            bool actual = false;

            MockEventOne testObj = new MockEventOne();

            actual = TableEventStreamBase.IsPropertyEmpty(typeof(MockEventOne).GetProperty(nameof(MockEventOne.dateTimeProperty)), testObj);

            Assert.AreEqual(expected, actual);

        }

        [TestMethod]
        public void IsPropertyEmpty_EmptyDateTimeOffset_TestMethod()
        {
            bool expected = true;
            bool actual = false;

            MockEventOne testObj = new MockEventOne();

            actual = TableEventStreamBase.IsPropertyEmpty(typeof(MockEventOne).GetProperty(nameof(MockEventOne.DateTimeOffsetProperty)), testObj);

            Assert.AreEqual(expected, actual);

        }

        [TestMethod]
        public void IsPropertyEmpty_EarlyDate_TestMethod()
        {
            bool expected = true;
            bool actual = false;

            MockEventOne testObj = new MockEventOne();
            testObj.dateTimeProperty = new DateTime(1290, 1, 1);

            actual = TableEventStreamBase.IsPropertyEmpty(typeof(MockEventOne).GetProperty(nameof(MockEventOne.dateTimeProperty)), testObj);

            Assert.AreEqual(expected, actual);

        }

        [TestMethod]
        public void IsPropertyEmpty_EarlyDateTimeOffset_TestMethod()
        {
            bool expected = true;
            bool actual = false;

            MockEventOne testObj = new MockEventOne();
            testObj.DateTimeOffsetProperty = new DateTimeOffset(new DateTime(1588, 12, 1));
            actual = TableEventStreamBase.IsPropertyEmpty(typeof(MockEventOne).GetProperty(nameof(MockEventOne.DateTimeOffsetProperty)), testObj);

            Assert.AreEqual(expected, actual);

        }

        [TestMethod]
        public void IsPropertyEmpty_LaterDateTimeOffset_TestMethod()
        {
            bool expected = false ;
            bool actual = true;

            MockEventOne testObj = new MockEventOne();
            testObj.DateTimeOffsetProperty = new DateTimeOffset(new DateTime(1988, 12, 1));
            actual = TableEventStreamBase.IsPropertyEmpty(typeof(MockEventOne).GetProperty(nameof(MockEventOne.DateTimeOffsetProperty)), testObj);

            Assert.AreEqual(expected, actual);

        }
    }

}

namespace Mocking
{
    public class MockEventOne
    {

        public int IntegerProperty { get; set; }

        public string StringProperty { get; set; }

        public DateTime dateTimeProperty { get; set; }

        public object objectProperty { get; set; }

        public DateTimeOffset DateTimeOffsetProperty { get; set; }
    }
}
