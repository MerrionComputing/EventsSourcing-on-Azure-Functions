using EventSourcingOnAzureFunctions.Common.Binding;
using EventSourcingOnAzureFunctions.Common.EventSourcing;
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

    }
}
