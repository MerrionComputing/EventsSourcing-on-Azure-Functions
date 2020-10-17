using EventSourcingOnAzureFunctions.Common.Notification;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;

namespace EventSourcingOnAzureFunctions.Common.EventSourcing.DependencyInjection
{
    /// <summary>
    /// Extensions for registering a notification dispatcher for dependnecy injection
    /// </summary>
    public static partial class NotificationServiceCollectionExtensions
    {

        public static IServiceCollection AddNotificationDispatch(this IServiceCollection services)
        {

            // Create the notification dispatcher singleton
            services.AddSingleton<INotificationDispatcher, EventGridNotificationDispatcher >();

            return services;
        }

    }
}
