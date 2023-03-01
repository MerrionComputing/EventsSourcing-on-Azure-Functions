using EventSourcingOnAzureFunctions.Common.CQRS;
using EventSourcingOnAzureFunctions.Common.CQRS.ClassifierHandler.Functions;
using EventSourcingOnAzureFunctions.Common.EventSourcing;
using EventSourcingOnAzureFunctions.Common.EventSourcing.Implementation;
using EventSourcingOnAzureFunctions.Common.Listener;
using EventSourcingOnAzureFunctions.Common.Binding;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace EventSourcingOnAzureFunctions.Test
{
    [TestClass]
    public class CommandHandler_UnitTest
    {

        [TestMethod]
        public void CommandStepTriggerAttribute_Constructor_TestMethod()
        {

            CommandStepTriggerAttribute step = new CommandStepTriggerAttribute("Test Domain",
                "Test Command",
                "Test Command State");

            Assert.IsNotNull(step);
        }

    }
}
