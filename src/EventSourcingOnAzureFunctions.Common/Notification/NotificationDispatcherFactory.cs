using EventSourcingOnAzureFunctions.Common.EventSourcing.Interfaces;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace EventSourcingOnAzureFunctions.Common.Notification
{
    public static class NotificationDispatcherFactory
    {

        /// <summary>
        /// The static factory-created notification dispatcher
        /// </summary>
        public static ConcurrentDictionary<string, INotificationDispatcher> NotificationDispatchers { get; private set; }

        public static INotificationDispatcher GetDispatcher(string dispatcherName)
        {

            if (NotificationDispatchers != null)
            {
                if (NotificationDispatchers.ContainsKey(dispatcherName ) )
                {
                    return NotificationDispatchers[dispatcherName];
                }
            }

            // If not found, return the NULL one for composability
            return new NullNotificationDispatcher();
        }

        /// <summary>
        /// A default dispatcher to use if none is specified
        /// </summary>
        /// <returns></returns>
        public  static INotificationDispatcher GetDefaultDispatcher()
        {
            return GetDispatcher(nameof(EventGridNotificationDispatcher));
        }


        /// <summary>
        /// Create any static classes used to dispatch notifications 
        /// </summary>
        /// <param name="services"></param>
        public static void CreateDispatchers(IServiceCollection services)
        {
            if (null != services )
            {
                INameResolver nameResolver = null;
                IOptions<EventSourcingOnAzureOptions> options = null;
                ILogger logger = null;
                IEventStreamSettings settings = null;

                var provider = services.BuildServiceProvider();
                using (var scope = provider.CreateScope()  )
                {
                    nameResolver =  scope.ServiceProvider.GetRequiredService<INameResolver>();
                    IConfiguration configuration = scope.ServiceProvider.GetRequiredService< IConfiguration > ();
                    if (null != configuration )
                    {
                        EventSourcingOnAzureOptions optionConfig = new EventSourcingOnAzureOptions(configuration);
                        options = Options.Create<EventSourcingOnAzureOptions>(optionConfig);
                        if (null == nameResolver)
                        {
                            // make a default name resolver
                            nameResolver = new NotificationDispatcherNameResolver(configuration );
                        }
                    }
                    // Get the event stream settings to use
                    settings = scope.ServiceProvider.GetRequiredService<IEventStreamSettings>(); 
                }

                if (null == options )
                {
                    // make a default set of options
                }

                

                // Add the default notification dispatchers
                if (NotificationDispatchers == null)
                {
                    NotificationDispatchers = new ConcurrentDictionary<string, INotificationDispatcher>();
                }
                EventGridNotificationDispatcher evDispatcher = new EventGridNotificationDispatcher(options, nameResolver, logger);
                if (evDispatcher != null)
                {
                    NotificationDispatchers.TryAdd(evDispatcher.Name, evDispatcher );
                }
                QueueNotificationDispatcher quDispatch = new QueueNotificationDispatcher(options, settings, logger);
                if (quDispatch != null)
                {
                    NotificationDispatchers.TryAdd(quDispatch.Name , quDispatch );
                }
                NullNotificationDispatcher nuDispatch = new NullNotificationDispatcher();
                if (nuDispatch != null)
                {
                    NotificationDispatchers.TryAdd(nuDispatch.Name, nuDispatch);
                }
            }
        }

        static NotificationDispatcherFactory()
        {
            // Build the services collection
            // if there is an <INotificationDispatcher> use it
            // otherwise create one and add it...

        }

    }
}
