using Microsoft.Azure.WebJobs;
using System;
using Microsoft.Azure.WebJobs.Hosting;

[assembly: WebJobsStartup(typeof(EventSourcingOnAzureFunctions.Common.WebjobsStartup))]
namespace EventSourcingOnAzureFunctions.Common
{
    public class WebjobsStartup
        : IWebJobsStartup
    {
        public void Configure(IWebJobsBuilder builder)
        {
            // Add the standard (built-in) bindings 
            builder.AddBuiltInBindings();

            // Allow the execution context to be accessed by DI
            builder.AddExecutionContextBinding();

            //Register any extensions for bindings
            builder.AddExtension<InjectConfiguration>()
                .BindOptions<EventSourcingOnAzureOptions>() ;

        }
    }
}
