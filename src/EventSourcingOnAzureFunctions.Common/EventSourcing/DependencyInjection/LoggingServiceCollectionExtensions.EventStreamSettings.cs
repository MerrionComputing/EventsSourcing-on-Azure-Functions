using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using EventSourcingOnAzureFunctions.Common.EventSourcing.Interfaces;

namespace EventSourcingOnAzureFunctions.Common.EventSourcing.DependencyInjection
{
    /// <summary>
    /// Extensions for dependency injection of the [EventStreamSettings]
    /// </summary>
    public static partial class LoggingServiceCollectionExtensions
    {

        public static IServiceCollection AddEventStreamSettings(this IServiceCollection services)
        {

            // and the Event Stream Settings singleton
            services.AddSingleton<IEventStreamSettings, EventStreamSettings>();

            return services;
        }
    }
}
