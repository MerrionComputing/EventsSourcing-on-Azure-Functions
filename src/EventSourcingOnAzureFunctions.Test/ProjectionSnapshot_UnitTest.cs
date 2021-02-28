using EventSourcingOnAzureFunctions.Common.EventSourcing;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace EventSourcingOnAzureFunctions.Test
{
    [TestClass]
    public class ProjectionSnapshot_UnitTest
    {

        [TestInitialize]
        public void InitialiseEnvironmentVariables()
        {
            EnvironmentVariables.SetTestVariables();
        }

    }



}
