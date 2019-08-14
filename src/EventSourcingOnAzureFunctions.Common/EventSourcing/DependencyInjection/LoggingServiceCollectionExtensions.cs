using Microsoft.Azure.WebJobs;
using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;

namespace EventSourcingOnAzureFunctions.Common.EventSourcing.DependencyInjection
{
    public static partial class LoggingServiceCollectionExtensions
    {
        private readonly static ExecutionContext context = new ExecutionContext();


    }
}
