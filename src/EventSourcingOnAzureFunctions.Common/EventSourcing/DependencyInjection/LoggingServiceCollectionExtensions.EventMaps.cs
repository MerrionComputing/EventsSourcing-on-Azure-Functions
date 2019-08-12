using EventSourcingOnAzureFunctions.Common.EventSourcing.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;

namespace EventSourcingOnAzureFunctions.Common.EventSourcing.DependencyInjection
{
    /// <summary>
    /// Extensions for dependency injection of the [EventMaps]
    /// </summary>
    public  static partial class LoggingServiceCollectionExtensions
    {

        public static IServiceCollection AddEventMaps(this IServiceCollection services)
        {

            // Create the event maps singleton
            services.AddSingleton<IEventMaps, EventMaps>();

            return services;
        }

    }
}
