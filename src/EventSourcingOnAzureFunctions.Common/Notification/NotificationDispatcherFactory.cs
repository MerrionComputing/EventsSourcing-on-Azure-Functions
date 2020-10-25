using EventSourcingOnAzureFunctions.Common.EventSourcing.Interfaces;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Collections.Generic;
using System.Linq;

namespace EventSourcingOnAzureFunctions.Common.Notification
{
    public static class NotificationDispatcherFactory
    {

        /// <summary>
        /// The static factory-created notification dispatcher
        /// </summary>
        public static IEnumerable<INotificationDispatcher> NotificationDispatchers { get; private set; }

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
                    NotificationDispatchers = new List<INotificationDispatcher>();
                }
                NotificationDispatchers.Append( new EventGridNotificationDispatcher(options, nameResolver, logger ));
                NotificationDispatchers.Append(new QueueNotificationDispatcher(options, settings, logger));
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
