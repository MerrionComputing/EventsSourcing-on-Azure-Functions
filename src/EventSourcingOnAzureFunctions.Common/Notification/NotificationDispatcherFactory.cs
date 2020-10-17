using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Linq;

namespace EventSourcingOnAzureFunctions.Common.Notification
{
    public static class NotificationDispatcherFactory
    {

        /// <summary>
        /// The static factory-created notification dispatcher
        /// </summary>
        public static INotificationDispatcher NotificationDispatcher { get; private set; }

        public static void CreateDispatcher(IServiceCollection services)
        {
            if (null != services )
            {
                INameResolver nameResolver = null;
                IOptions<EventSourcingOnAzureOptions> options = null;
                ILogger logger = null;

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
                }

                if (null == options )
                {
                    // make a default set of options
                }

                NotificationDispatcher = new EventGridNotificationDispatcher(options, nameResolver, logger ); 
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
