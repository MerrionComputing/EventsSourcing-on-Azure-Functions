using EventSourcingOnAzureFunctions.Common.EventSourcing;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using System.Reflection;

namespace EventSourcingOnAzureFunctions.Test
{
    [TestClass ]
    public class EventMaps_UnitTest
    {

        [TestInitialize]
        public void EventMaps_UnitTest_Initialise()
        {
            // set the path to be local...
            Environment.SetEnvironmentVariable("AzureWebJobsScriptRoot", Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location));
        }


        [TestMethod]
        public void Constructor_TestMethod()
        {

            EventMaps testObj = new EventMaps();

            Assert.IsNotNull(testObj);  

        }

        [TestMethod]
        public void LoadFrom_Config_TestMethod()
        {

            EventMaps testObj = new EventMaps();
            testObj.LoadFromConfig();

            var evtClass = testObj.CreateEventClass("Mock Event One");
            Assert.IsNotNull(evtClass); 
            
        }

        [TestMethod]
        public void LoadFrom_Reflection_TestMethod()
        {

            EventMaps testObj = new EventMaps();
            testObj.LoadByReflection();

            var evtClass = testObj.CreateEventClass("Mock Event One");
            Assert.IsNotNull(evtClass);

        }

        [TestMethod]
        public void LoadFrom_Reflection_Deposit_TestMethod()
        {

            EventMaps testObj = new EventMaps();
            testObj.LoadByReflection();

            var evtClass = testObj.CreateEventClass("Money Deposited");
            Assert.IsNotNull(evtClass);

        }

        [TestMethod]
        public void LoadFrom_Reflection_Withdrawal_TestMethod()
        {

            EventMaps testObj = new EventMaps();
            testObj.LoadByReflection();

            var evtClass = testObj.CreateEventClass("Money Withdrawn");
            Assert.IsNotNull(evtClass);

        }

    }
}
