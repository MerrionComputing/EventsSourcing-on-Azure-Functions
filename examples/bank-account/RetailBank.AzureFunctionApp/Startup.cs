using EventSourcingOnAzureFunctions.Common;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host.Config;
using Microsoft.Azure.WebJobs.Hosting;
using System;
using System.Collections.Generic;
using System.Text;


[assembly: WebJobsStartup(typeof(RetailBank.AzureFunctionApp.AzureFunctionAppStartup),
    "Retail Bank CQRS on Azure Demo")]
namespace RetailBank.AzureFunctionApp
{


    /// <summary>
    /// Start-up class to load all the dependency injection and startup configuration code
    /// </summary>
    public class AzureFunctionAppStartup
        : EventSourcingOnAzureFunctions.Common.Startup
    {
        public new void Configure(IWebJobsBuilder builder)
        {

            // Initialise any common services
            base.Configure(builder);

        }
    }

}
