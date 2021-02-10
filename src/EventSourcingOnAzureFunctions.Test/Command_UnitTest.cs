using EventSourcingOnAzureFunctions.Common.Binding;
using EventSourcingOnAzureFunctions.Common.CQRS;
using EventSourcingOnAzureFunctions.Common.CQRS.CommandHandler;
using EventSourcingOnAzureFunctions.Common.EventSourcing;
using EventSourcingOnAzureFunctions.Common.EventSourcing.Implementation;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace EventSourcingOnAzureFunctions.Test
{
    [TestClass]
    public class Command_UnitTest
    {

        /// <summary>
        /// Set up the environment variables required to run these tests
        /// </summary>
        [TestInitialize]
        public void EventStream_UnitTest_Initialise()
        {
            Environment.SetEnvironmentVariable("Bank.Account", "Table;RetailBank");
            Environment.SetEnvironmentVariable("Bank.Command.Pay Accrued Interest", "Table;RetailBank");
            //ALL.ALL=Table;RetailBank
            Environment.SetEnvironmentVariable("ALL.ALL", "Table;RetailBank");

            // set the path to be local...
            Environment.SetEnvironmentVariable("AzureWebJobsScriptRoot", 
                Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location));

        }

        [TestMethod]
        public void CommandAttribute_Constructor_TestMethod()
        {
            CommandAttribute testObj = new CommandAttribute("Bank", "Apply Interest", "1234567");
            Assert.IsNotNull(testObj);

        }
    }
}
