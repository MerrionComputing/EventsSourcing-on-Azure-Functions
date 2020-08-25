using EventSourcingOnAzureFunctions.Common.CQRS;
using EventSourcingOnAzureFunctions.Common.CQRS.ProjectionHandler.Functions;
using EventSourcingOnAzureFunctions.Common.EventSourcing;
using EventSourcingOnAzureFunctions.Common.EventSourcing.Implementation;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Threading.Tasks;

namespace EventSourcingOnAzureFunctions.Test
{
    [TestClass ]
    public class ProjectionHandlerFunctions_UnitTest
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
        public void ProjectionRequestedEventGridEventData_WithData_TestMethod()
        {

            ProjectionRequestedEventGridEventData testObj = new ProjectionRequestedEventGridEventData()
            {
                Commentary = "This is a unit test",
                DomainName = "Bank",
                EntityTypeName = "Query",
                InstanceKey = "QRY-1234-6567",
                ProjectionRequest = 
                   new Common.CQRS.ProjectionHandler.Events.ProjectionRequested()
                    { 
                       ProjectionDomainName = "Bank",
                       ProjectionEntityTypeName = "Account",
                       ProjectionInstanceKey = "A-001-223456-B"
                   }
            };

            Assert.IsNotNull(testObj);
        }

        [TestMethod]
        public async Task RunProjectionForQuery_TestMethod()
        {

            Query testQuery = new Query("Bank",
                "Get Available Balance",
                "QRY-TEST-A0001"
                );

            // Add a projection-requested event
            await testQuery.RequestProjection("Bank",
                       "Account",
                       "A-001-223456-B",
                       "Balance",
                       null);

            // Perform the projection request
            ProjectionRequestedEventGridEventData projReq = new ProjectionRequestedEventGridEventData()
            {
                Commentary = "This is a unit test",
                DomainName = testQuery.DomainName ,
                EntityTypeName = testQuery.QueryName ,
                InstanceKey = testQuery.UniqueIdentifier ,
                ProjectionRequest =
                   new Common.CQRS.ProjectionHandler.Events.ProjectionRequested()
                   {
                       ProjectionDomainName = "Domain Test",
                       ProjectionEntityTypeName = "Entity Type Test",
                       ProjectionInstanceKey = "Instance 123",
                       ProjectionTypeName = "Mock Projection One"
                   }
            };

            await ProjectionHandlerFunctions.RunProjectionForQuery(projReq);

            // TODO: Now check that there was a projection response...

            Assert.IsNotNull(testQuery);

        }

    }
}
