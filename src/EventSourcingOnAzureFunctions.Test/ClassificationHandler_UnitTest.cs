using EventSourcingOnAzureFunctions.Common.CQRS;
using EventSourcingOnAzureFunctions.Common.CQRS.ClassifierHandler.Functions;
using EventSourcingOnAzureFunctions.Common.EventSourcing;
using EventSourcingOnAzureFunctions.Common.EventSourcing.Implementation;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;
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
                ClassifierRequest =  new Common.CQRS.ClassifierHandler.Events.ClassifierRequested()
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


        [TestMethod]
        public async Task RunClassificationForQuery_TestMethod()
        {

            int expected = 1;
            int actual = 0;

            Query testQuery = new Query("Bank",
                "Get Available Balance",
                "QRY-TEST-A0007"
                );

            // Add a projection-requested event
            await testQuery.RequestClassification("Bank",
                       "Account",
                       "A-001-223456-B",
                       "In Credit",
                       null,
                       null);

            // Perform the projection request
            ClassifierRequestedEventGridEventData clsReq = new ClassifierRequestedEventGridEventData()
            {
                Commentary = "This is a unit test",
                DomainName = testQuery.DomainName,
                EntityTypeName = testQuery.QueryName,
                InstanceKey = testQuery.UniqueIdentifier,
                ClassifierRequest =
                   new Common.CQRS.ClassifierHandler.Events.ClassifierRequested()
                   {
                       DomainName = "Bank",
                       EntityTypeName = "Account",
                       InstanceKey = "A-001-223456-B",
                       ClassifierTypeName = "In Credit"
                   }
            };

            await ClassifierHandlerFunctions.RunClassificationForQuery(clsReq);

            // Add an unprocessed request so that we have something to verify was not processed
            await testQuery.RequestClassification("Bank",
                       "Account",
                       "B-001-223456-B",
                       "In Credit",
                       null,
                       null);

            // Now check that there was a classifier response...
            var outstanding = await testQuery.GetOutstandingClassifiers();
            if (outstanding != null)
            {
                actual = outstanding.Count();
            }

            Assert.AreEqual(expected, actual);

        }

    }
}
