﻿using EventSourcingOnAzureFunctions.Common.Notification;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Azure.WebJobs;

[assembly: FunctionsStartup(typeof(EventSourcingOnAzureFunctions.Common.Startup))]
namespace EventSourcingOnAzureFunctions.Common
{
    public class Startup
        : FunctionsStartup
    {

        public override void Configure(IFunctionsHostBuilder builder)
        {

            builder.AddAppSettingsToConfiguration();
 
            // Initialise any common servicesconfigureservices
            CQRSAzureBindings.InitializeServices(builder.Services);

            // Initialise any outbound notifications
            NotificationDispatcherFactory.CreateDispatcher(builder.Services); 

            // Initialise any inbound listeners

        }

    }


}
