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

        [Ignore("Config loaded from environment strings") ]
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
            testObj.InitialiseEnvironmentStrings();

            actual = testObj.GetBackingImplementationType(new EventStreamAttribute("Domain Test", "Entity Type Test", "Instance 123"));

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void CreateWriter_Table__TestMethod()
        {


            EventStreamSettings testObj = new EventStreamSettings();
            testObj.InitialiseEnvironmentStrings();

            var writerObj = testObj.CreateWriterForEventStream(new EventStreamAttribute("Domain Test", "Entity Type Test", "Instance 123"));

            Assert.IsInstanceOfType(writerObj, typeof(TableEventStreamWriter));
        }

        [TestMethod]
        public void CreateProjectionProcessor_Table__TestMethod()
        {


            EventStreamSettings testObj = new EventStreamSettings();
            testObj.InitialiseEnvironmentStrings();

            var readerObj = testObj.CreateProjectionProcessorForEventStream(new ProjectionAttribute("Bank", "Account", "Instance 123", "Balance"));

            Assert.IsNotNull (readerObj);
        }

        [TestMethod]
        public void MakeEnvironmentStringKey_Short_TestMethod()
        {

            string expected = "Bank.Account";
            string actual = "Not set";

            actual = EventStreamSetting.MakeEnvironmentStringKey(new EventStreamAttribute("Bank", "Account", "A94N6X6"));

            Assert.AreEqual(expected, actual); 

        }

        [TestMethod]
        public void MakeEnvironmentStringKey_Long_TestMethod()
        {

            string expected = "Conglomereate.EMEA.France.Paris.Bank.Account";
            string actual = "Not set";

            actual = EventStreamSetting.MakeEnvironmentStringKey(new EventStreamAttribute("Conglomereate.EMEA.France.Paris.Bank", "Account", "A94N6X6"));

            Assert.AreEqual(expected, actual);

        }

        [TestMethod]
        public void MakeEnvironmentStringKey_TooLong_TestMethod()
        {

            string expected = "ABCDEFGHIJKLMNOPQRSTUVWXYZ.abcdefghijklmnopqrstuvwxyz.ABCDEFGHIJKLMNOPQRSTUVWXYZ.abcdefghijklmnopqrstuvwxyz.ABCDEFGHIJKLMNOPQRSTUVWXYZ.abcdefghijklmnopqrstuvwxyz.ABCDEFGHIJKLMNOPQRSTUVWXYZ.abcdefghijklmnopqrstuvwxyz.ABCDEFGHIJKLMNOPQRSTUVWX-1946694419";
            string actual = "Not set";

            actual = EventStreamSetting.MakeEnvironmentStringKey(new EventStreamAttribute("ABCDEFGHIJKLMNOPQRSTUVWXYZ.abcdefghijklmnopqrstuvwxyz.ABCDEFGHIJKLMNOPQRSTUVWXYZ.abcdefghijklmnopqrstuvwxyz.ABCDEFGHIJKLMNOPQRSTUVWXYZ.abcdefghijklmnopqrstuvwxyz.ABCDEFGHIJKLMNOPQRSTUVWXYZ.abcdefghijklmnopqrstuvwxyz.ABCDEFGHIJKLMNOPQRSTUVWXYZ.abcdefghijklmnopqrstuvwxyz.ABCDEFGHIJKLMNOPQRSTUVWXYZ.abcdefghijklmnopqrstuvwxyz.ABCDEFGHIJKLMNOPQRSTUVWXYZ.abcdefghijklmnopqrstuvwxyz.ABCDEFGHIJKLMNOPQRSTUVWXYZ.abcdefghijklmnopqrstuvwxyz.ABCDEFGHIJKLMNOPQRSTUVWXYZ.abcdefghijklmnopqrstuvwxyz.ABCDEFGHIJKLMNOPQRSTUVWXYZ.abcdefghijklmnopqrstuvwxyz.ABCDEFGHIJKLMNOPQRSTUVWXYZ.abcdefghijklmnopqrstuvwxyz"
                , "Account", "A94N6X6"));

            Assert.AreEqual(expected, actual);

        }

        [TestMethod]
        public void SettingsFromEnvironmentStringValue_Table_TestMethod()
        {

            string expected = @"Table";
            string actual = "Not set";

            string environmentvarialble = "Table;MarketLibraryConnectionString";

            var setting = EventStreamSetting.SettingsFromEnvironmentStringValue(new EventStreamAttribute("Domain Test", "Entity Type Test", "Instance 123"),
                environmentvarialble);

            actual = setting.Storage;

            Assert.AreEqual(actual, expected);
            


        }

        [TestMethod]
        public void SettingsFromEnvironmentStringValue_MarketLibraryConnectionString_TestMethod()
        {

            string expected = @"MarketLibraryConnectionString";
            string actual = "Not set";

            string environmentvarialble = "Table;MarketLibraryConnectionString";

            var setting = EventStreamSetting.SettingsFromEnvironmentStringValue(new EventStreamAttribute("Domain Test", "Entity Type Test", "Instance 123"),
                environmentvarialble);

            actual = setting.ConnectionStringName;

            Assert.AreEqual(actual, expected);



        }

        [TestMethod]
        public void SettingsFromEnvironmentStringValue_Empty_TestMethod()
        {

            string expected = @"AppendBlob";
            string actual = "Not set";

            string environmentvarialble = "";

            var setting = EventStreamSetting.SettingsFromEnvironmentStringValue(new EventStreamAttribute("Domain Test", "Entity Type Test", "Instance 123"),
                environmentvarialble);

            actual = setting.Storage;

            Assert.AreEqual(actual, expected);

        }

        [TestMethod]
        public void SettingsFromEnvironmentStringValue_EmptyConnectionStringName_TestMethod()
        {

            string expected = @"Domain Test.Entity Type Test.StorageConnectionString";
            string actual = "Not set";

            string environmentvarialble = "";

            var setting = EventStreamSetting.SettingsFromEnvironmentStringValue(new EventStreamAttribute("Domain Test", "Entity Type Test", "Instance 123"),
                environmentvarialble);

            actual = setting.ConnectionStringName ;

            Assert.AreEqual( expected, actual);

        }
    }
}
