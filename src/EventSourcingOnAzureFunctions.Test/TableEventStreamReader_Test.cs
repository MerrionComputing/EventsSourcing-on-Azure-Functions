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

        [TestInitialize]
        public void InitialiseEnvironmentVariables()
        {
            DotNetEnv.Env.Load();
        }

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
                new EventStreamAttribute("Bank", "Account", "Instance 1234"),
                "RetailBank");

            ProjectionProcessor testObj = new ProjectionProcessor(testReader);

            actual = await testObj.Exists();

            Assert.AreEqual(expected, actual);

        }

        [TestMethod]
        public async Task Projection_NotExists_TestMethod()
        {
            bool expected = false;
            bool actual = true;

            TableEventStreamReader testReader = new TableEventStreamReader(
                new EventStreamAttribute("Domain Test", "Entity Type Test", "Instance is not existing abc.def"),
                "RetailBank");

            ProjectionProcessor testObj = new ProjectionProcessor(testReader);

            actual = await testObj.Exists();

            Assert.AreEqual(expected, actual);

        }

        [TestMethod]
        public void Projection_MockProjectionOne_TestMethod()
        {

            decimal notexpected = 0;
            decimal actual = 0;

            TableEventStreamReader testReader = new TableEventStreamReader(
                new EventStreamAttribute("Bank", "Account", "Instance 1234"),
                "RetailBank");

            ProjectionProcessor testObj = new ProjectionProcessor(testReader);

            var result = testObj.Process<MockBalanceProjection>();
            actual = result.Result.CurrentBalance;

            Assert.AreNotEqual(notexpected, actual);

        }

        [TestMethod]
        public void Projection_MockBalanceProjection_TestMethod()
        {

            decimal notexpected = 0;
            decimal actual = 0;

            TableEventStreamReader testReader = new TableEventStreamReader(
                new EventStreamAttribute("Bank", "Account", "Instance 1234"),
                "RetailBank");

            ProjectionProcessor testObj = new ProjectionProcessor(testReader);

            MockBalanceProjection prior = new MockBalanceProjection();
            // start from event # 2
            prior.SetLastEventSequence(274);
            prior.SetInitialBalance(123.45M);

            var result = testObj.Process<MockBalanceProjection>(prior);
            actual = result.Result.CurrentBalance ;

            Assert.AreNotEqual(notexpected, actual);

        }

        [TestMethod]
        public async Task AllKeys_TestMethod()
        {

            TableEventStreamReader testReader = new TableEventStreamReader(
                new EventStreamAttribute("Bank", "Account", "Instance 123"),
                "RetailBank");

            var allKeys = await testReader.GetAllInstanceKeys(null);

            Assert.IsNotNull(allKeys); 
        }
    }
}

namespace Mocking
{

    [ProjectionName("Mock Projection One") ]
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

        public override string ToString()
        {
            return $"Total : {intCount}, {lastMessage}";
        }
    }


    [ProjectionName("Balance")]
    public class MockBalanceProjection
        : ProjectionBase,
        IHandleEventType<MockWithdrawal >,
        IHandleEventType<MockDeposit >
    {

        decimal _currentBalance = 0.00M;

        /// <summary>
        /// The current balance after the projection has run over a bank account event stream
        /// </summary>
        public decimal CurrentBalance
        {
            get
            {
                return _currentBalance;
            }
        }

        string _lastMessage = "";

        public string LastMessage
        {
            get
            {
                return _lastMessage;
            }
        }

        /// <summary>
        /// Pretend the projection has run to a biven event number
        /// </summary>
        /// <param name="sequenceNumber">
        /// The sequence number we have run up to
        /// </param>
        /// <remarks>
        /// This is to allow failing tests to check concurrency protection and out-of-sequence
        /// event checks
        /// </remarks>
        public void SetLastEventSequence(int sequenceNumber)
        {
            base.OnEventRead(sequenceNumber, null);
            base.MarkEventHandled(sequenceNumber); 
        }

        /// <summary>
        /// Set a starting balance to "pretend" we have already run some of the projection
        /// </summary>
        /// <param name="startingBalance">
        /// The initial balance to set
        /// </param>
        public void SetInitialBalance(decimal startingBalance)
        {
            _currentBalance = startingBalance;
        }

        public void HandleEventInstance(MockWithdrawal eventInstance)
        {
            if (eventInstance != null)
            {
                _currentBalance -= eventInstance.AmountWithdrawn;
                if (!string.IsNullOrWhiteSpace(eventInstance.Commentary))
                {
                    _lastMessage = eventInstance.Commentary;
                }
            }
        }

        public void HandleEventInstance(MockDeposit eventInstance)
        {
            if (eventInstance != null)
            {
                _currentBalance += eventInstance.AmountDeposited;
                if (!string.IsNullOrWhiteSpace(eventInstance.Commentary))
                {
                    _lastMessage = eventInstance.Commentary;
                }
            }
        }
    }
}
