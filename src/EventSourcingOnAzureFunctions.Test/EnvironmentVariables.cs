using System;
using System.Collections.Generic;
using System.Text;

namespace EventSourcingOnAzureFunctions.Test
{
    /// <summary>
    /// Set the environment variables for mock testing
    /// </summary>
    public sealed class EnvironmentVariables
    {
        public static void SetTestVariables()
        {
            Environment.SetEnvironmentVariable("Bank.Account", "Table;RetailBank");
            Environment.SetEnvironmentVariable("Domain Test.Entity Type Test", "Table;RetailBank");
            Environment.SetEnvironmentVariable("Domain Test.Entity Type Test Two", "AppendBlob;CosmosEmulator");
        }
    }
}
