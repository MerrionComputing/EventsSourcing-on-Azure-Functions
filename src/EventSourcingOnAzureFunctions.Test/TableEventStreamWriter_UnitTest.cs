using EventSourcingOnAzureFunctions.Common.Binding;
using EventSourcingOnAzureFunctions.Common.EventSourcing;
using EventSourcingOnAzureFunctions.Common.EventSourcing.Exceptions;
using EventSourcingOnAzureFunctions.Common.EventSourcing.Implementation.AzureStorage.Table;
using EventSourcingOnAzureFunctions.Common.EventSourcing.Interfaces;
using Microsoft.VisualStudio.TestPlatform.CommunicationUtilities.Serialization;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Mocking;
using System;
using System.Threading.Tasks;

namespace EventSourcingOnAzureFunctions.Test
{
    [TestClass]
    public class TableEventStreamWriter_UnitTest
    {

        [TestInitialize]
        public void InitialiseEnvironmentVariables()
        {
            EnvironmentVariables.SetTestVariables();
        }

        [TestMethod]
        public void Constructor_TestMethod()
        {
            TableEventStreamWriter testObj = new TableEventStreamWriter(new EventStreamAttribute("Domain Test", "Entity Type Test", "Instance 123"),
                "RetailBank");

            Assert.IsNotNull(testObj);

        }

        [TestMethod]
        public async Task AppendEvent_Unconstrained_TestMethod()
        {

            TableEventStreamWriter testObj = new TableEventStreamWriter(new EventStreamAttribute("Domain Test", "Entity Type Test", "Instance 123"),
                "RetailBank");

            MockEventOne testEvent = new MockEventOne() { EventTypeName = "Test Event Happened" };
            testEvent.EventPayload = new MockEventOnePayload() { StringProperty = "This is some data", IntegerProperty = 123 };

           await testObj.AppendEvent(eventInstance: testEvent );

            Assert.IsNotNull(testObj); 
        }

        [TestMethod]
        public async Task AppendEvent__Instance2_Unconstrained_TestMethod()
        {

            TableEventStreamWriter testObj = new TableEventStreamWriter(new EventStreamAttribute("Domain Test", "Entity Type Test", "Instance 456"),
                "RetailBank");

            MockEventOne testEvent = new MockEventOne() { EventTypeName = "Testing Event" };
            testEvent.EventPayload = new MockEventOnePayload() { StringProperty = "This is some data for the new instance", IntegerProperty = 234 , dateTimeProperty = DateTime.UtcNow  };

            await testObj.AppendEvent(eventInstance: testEvent);

            Assert.IsNotNull(testObj);
        }

        [TestMethod]
        public async Task AppendEvent_MustExist_TestMethod()
        {

            TableEventStreamWriter testObj = new TableEventStreamWriter(new EventStreamAttribute("Domain Test", "Entity Type Test", "Instance 123"),
                "RetailBank");

            MockEventOne testEvent = new MockEventOne() { EventTypeName = "Test Event Happened" };
            testEvent.EventPayload = new MockEventOnePayload() { StringProperty = "This is some data", IntegerProperty = 123 };

            await testObj.AppendEvent(eventInstance: testEvent, 
                streamConstraint: Common.EventSourcing.Implementation.EventStreamBase.EventStreamExistenceConstraint.MustExist);

            Assert.IsNotNull(testObj);
        }

        [TestMethod]
        public async Task AppendEvent_Deposit_MustExist_TestMethod()
        {

            EventStream testObj = new EventStream(new EventStreamAttribute( "Bank", "Account", "Instance 1234"));

            MockDeposit  testEvent = new MockDeposit() { AmountDeposited = 22.99M, Commentary = "Unit testing" };
 
            await testObj.AppendEvent(testEvent,
                streamConstraint: Common.EventSourcing.Implementation.EventStreamBase.EventStreamExistenceConstraint.MustExist);

            Assert.IsNotNull(testObj);
        }

        [TestMethod]
        public async Task AppendEvent_Withdrawal_MustExist_TestMethod()
        {

            EventStream testObj = new EventStream(new EventStreamAttribute("Bank", "Account", "Instance 1234"));

            MockWithdrawal  testEvent = new MockWithdrawal() { AmountWithdrawn  = 10.99M, Commentary = "Unit testing withdrawal" };

            await testObj.AppendEvent(testEvent,
                streamConstraint: Common.EventSourcing.Implementation.EventStreamBase.EventStreamExistenceConstraint.MustExist);

            Assert.IsNotNull(testObj);
        }

        [TestMethod]
        [ExpectedException(typeof(EventStreamWriteException))]
        public async Task AppendEvent_MustExist_Fail_TestMethod()
        {

            TableEventStreamWriter testObj = new TableEventStreamWriter(new EventStreamAttribute("Domain Test", "Entity Type Test", "Instance does not exist 123.456.998"),
                "RetailBank");

            MockEventOne testEvent = new MockEventOne() { EventTypeName = "Test Event Happened" };
            testEvent.EventPayload = new MockEventOnePayload() { StringProperty = "This is some data", IntegerProperty = 123 };

            await testObj.AppendEvent(eventInstance: testEvent,
                streamConstraint: Common.EventSourcing.Implementation.EventStreamBase.EventStreamExistenceConstraint.MustExist);

            Assert.IsNotNull(testObj);
        }

        [TestMethod]
        [ExpectedException(typeof(EventStreamWriteException))]
        public async Task AppendEvent_ConcurrencyCrocodile_Fail_TestMethod()
        {

            TableEventStreamWriter testObj = new TableEventStreamWriter(new EventStreamAttribute("Bank", "Account", "Instance 1234"),
                "RetailBank");

            MockEventOne testEvent = new MockEventOne() { EventTypeName = "Test Event Happened" };
            testEvent.EventPayload = new MockEventOnePayload() { StringProperty = "This is some data", IntegerProperty = 123 };

            await testObj.AppendEvent(eventInstance: testEvent, 
                expectedTopSequenceNumber: 2);

            Assert.IsNotNull(testObj);
        }

        [TestMethod]
        [ExpectedException(typeof(EventStreamWriteException))]
        public async Task AppendEvent_MustNotExist_Fail_TestMethod()
        {

            TableEventStreamWriter testObj = new TableEventStreamWriter(new EventStreamAttribute("Bank", "Account", "Instance 1234"),
                "RetailBank");

            MockEventOne testEvent = new MockEventOne() { EventTypeName = "Test Event Happened" };
            testEvent.EventPayload = new MockEventOnePayload() { StringProperty = "This is some data", IntegerProperty = 123 };

            await testObj.AppendEvent(eventInstance: testEvent,
                streamConstraint: Common.EventSourcing.Implementation.EventStreamBase.EventStreamExistenceConstraint.MustBeNew);

            Assert.IsNotNull(testObj);
        }

        [TestMethod]
        [ExpectedException(typeof(EventStreamWriteException))]
        public async Task AppendEvent_Constrained_TestMethod()
        {

            TableEventStreamWriter testObj = new TableEventStreamWriter(new EventStreamAttribute("Domain Test", "Entity Type Test", "Instance 123"),
                "RetailBank");

            MockEventOne testEvent = new MockEventOne() { EventTypeName = "Failing event" };
            testEvent.EventPayload = new MockEventOnePayload() { StringProperty = "This is some more data", IntegerProperty = 123 };

            // This is meant to fail as there are more than 1 events in this stream
            await testObj.AppendEvent(eventInstance: testEvent, 
                expectedTopSequenceNumber: 1);

            Assert.IsNotNull(testObj);
        }


    }
}
