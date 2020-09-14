using EventSourcingOnAzureFunctions.Common.CQRS;
using EventSourcingOnAzureFunctions.Common.CQRS.ClassifierHandler.Functions;
using EventSourcingOnAzureFunctions.Common.EventSourcing;
using EventSourcingOnAzureFunctions.Common.EventSourcing.Implementation;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Threading.Tasks;

namespace EventSourcingOnAzureFunctions.Test
{
    [TestClass]
    public class ClassificationHandler_UnitTest
    {

        [TestInitialize]
        public void EventStream_UnitTest_Initialise()
        {
            Environment.SetEnvironmentVariable("Bank.Account", "Table;RetailBank");
            Environment.SetEnvironmentVariable("Bank.Query.Get Available Balance", "Table;RetailBank");
            //ALL.ALL=Table;RetailBank
            Environment.SetEnvironmentVariable("ALL.ALL", "Table;RetailBank");
        }

        [TestMethod]
        public void ClassificationRequestEventGridData_WithData_TestMethod()
        {
            ClassifierRequestedEventGridEventData testObj = new ClassifierRequestedEventGridEventData()
            { 
                Commentary = "This is a unit test classification request",
                DomainName = "Bank",
                EntityTypeName = "Query",
                InstanceKey = "QRY-1234-6567",
                ClassifierRequest =  new Common.ClassifierHandler.Events.ClassifierRequested()
                {
                     ClassifierTypeName = "Balance Below Threshold",
                     DomainName = "Bank",
                     EntityTypeName = "Account",
                     InstanceKey = "A-001-223456-B",
                     CorrelationIdentifier = "Test-123"
                     // Parameters ..?
                }
            };

            Assert.IsNotNull(testObj); 
        }

    }
}
