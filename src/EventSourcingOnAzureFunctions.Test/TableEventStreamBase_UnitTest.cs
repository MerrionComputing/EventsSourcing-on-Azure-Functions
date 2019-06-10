using EventSourcingOnAzureFunctions.Common.EventSourcing;
using EventSourcingOnAzureFunctions.Common.EventSourcing.Implementation.AzureStorage.Table;
using Microsoft.VisualStudio.TestTools.UnitTesting;


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


    }
}
