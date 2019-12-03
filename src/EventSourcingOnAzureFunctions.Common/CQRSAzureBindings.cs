using System;
using System.Threading.Tasks;
using EventSourcingOnAzureFunctions.Common.Binding;
using EventSourcingOnAzureFunctions.Common.EventSourcing;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Azure.WebJobs.Host.Bindings;
using Microsoft.Azure.WebJobs.Host.Config;
using Microsoft.Extensions.DependencyInjection;
using EventSourcingOnAzureFunctions.Common.EventSourcing.DependencyInjection;
using Microsoft.Extensions.Configuration;
using System.Linq;
using Microsoft.Extensions.DependencyInjection.Extensions;
using EventSourcingOnAzureFunctions.Common.EventSourcing.Interfaces;
using EventSourcingOnAzureFunctions.Common.EventSourcing.Implementation;
using EventSourcingOnAzureFunctions.Common.Notification;

namespace EventSourcingOnAzureFunctions.Common
{

    /// <summary>
    /// Bindings to use for the different event sourcing extensions
    /// </summary>
    public static class CQRSAzureBindings
    {


        /// <summary>
        /// Initialise any common services for dependency injection
        /// </summary>
        /// <param name="services">
        /// The service collection to which common services can be added
        /// </param>
        public static void InitializeServices(IServiceCollection services)
        {
            // Add logging services 
            services.AddLogging();
  
            // Add the defined event stream settings
            services.AddEventStreamSettings();

            // Add the event maps
            services.AddEventMaps();

            // Add notifications
            services.AddNotificationDispatch();

            // Add listeners
        }

        /// <summary>
        /// Initialise any common dependency injection configuration settings
        /// </summary>
        public static void InitializeInjectionConfiguration(ExtensionConfigContext context)
        {


            // Set up the dependency injection stuff for the custom bindings 
            // 1: EventStream
            context
                .AddBindingRule<EventStreamAttribute>()
                .BindToInput<EventStream>(BuildEventStreamFromAttribute)
                ;


            // 2: Projection
            context
              .AddBindingRule<ProjectionAttribute>()
              .BindToInput<Projection>(BuildProjectionFromAttribute)
              ;

            // 3: Classification
            context
              .AddBindingRule<ClassificationAttribute>()
              .BindToInput<Classification>(BuildClassificationFromAttribute)
              ;

            //4: EventTrigger 
            // TODO..

        }




        /// <summary>
        /// Create a new event stream from the attribute passed in
        /// </summary>
        /// <param name="attribute">
        /// The EventStreamAttribute tagging the parameter to crreate by dependency injection
        /// </param>
        /// <param name="context">
        /// The context within which this binding is occuring
        /// </param>
        /// <returns>
        /// A task that can create an event stream when required
        /// </returns>
        public static Task<EventStream> BuildEventStreamFromAttribute(EventStreamAttribute attribute,
            ValueBindingContext context)
        {

            // If possible get the event stream settings to use

            // If possible, get the write context to use
            IWriteContext writeContext = WriteContext.CreateFunctionContext( context.FunctionContext);

            // If possible, get the notification dipatcher to use
            INotificationDispatcher dispatcher = NotificationDispatcherFactory.NotificationDispatcher;

            // Use this and the attribute to create a new event stream instance
            return Task<EventStream>.FromResult(new EventStream(attribute, 
                context: writeContext,
                dispatcher: dispatcher ));
        }



        public static Task<Projection> BuildProjectionFromAttribute(ProjectionAttribute attribute,
            ValueBindingContext context)
        {
            // If possible get the connection string to use

            // Use this and the attribute to create a new classifier instance
            return Task<Projection>.FromResult(new Projection(attribute));
        }

        public static Task<Classification> BuildClassificationFromAttribute(ClassificationAttribute attribute,
            ValueBindingContext context)
        {
            // If possible get the connection string to use

            // Use this and the attribute to create a new classifier instance
            return Task<Classification>.FromResult(new Classification (attribute));
        }

        public static IFunctionsHostBuilder AddAppSettingsToConfiguration(this IFunctionsHostBuilder builder)
        {
  

            var currentDirectory = "/home/site/wwwroot";
            bool isLocal = string.IsNullOrEmpty(Environment.GetEnvironmentVariable("WEBSITE_INSTANCE_ID"));
            if (isLocal)
            {
                currentDirectory = Environment.CurrentDirectory;
            }

            var tmpConfig = new ConfigurationBuilder()
                .SetBasePath(currentDirectory)
                .AddJsonFile("appsettings.json")
                .AddEnvironmentVariables() 
                .Build();

            string environmentName = tmpConfig["Environment"];
            if (string.IsNullOrEmpty(environmentName) )
            {
                // default to production
                environmentName = "production";
            }

            var configurationBuilder = new ConfigurationBuilder();

            var descriptor = builder.Services.FirstOrDefault(d => d.ServiceType == typeof(IConfiguration));
            if (descriptor?.ImplementationInstance is IConfiguration configRoot)
            {
                configurationBuilder.AddConfiguration(configRoot);
            }

            var configuration = configurationBuilder.SetBasePath(currentDirectory)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile($"appsettings.{environmentName}.json", optional: true, reloadOnChange: true)
                .AddJsonFile("connectionstrings.json", optional: true)
                .AddJsonFile("eventmaps.json", optional: true, reloadOnChange: true)
                .AddJsonFile("eventstreamsettings.json", optional: true, reloadOnChange: true)
                .AddEnvironmentVariables() 
                .Build();

            if (builder.Services.Contains(ServiceDescriptor.Singleton(typeof(IConfiguration))))
            {
                builder.Services.Replace(ServiceDescriptor.Singleton(typeof(IConfiguration), configuration));
            }
            else
            {
                builder.Services.AddSingleton<IConfiguration>(configuration);
            }
            return builder;
        }

        /// <summary>
        /// Settings that can be set for an append blob backed event stream
        /// </summary>
        /// <remarks>
        /// ImplementationType=AzureBlob;ConnectionStringName=LeaguesConnectionString;DomainName=Leagues
        /// </remarks>
        public class AppendBlobConnectionSettings
            : ConnectionSettingsBase
        {


            public AppendBlobConnectionSettings(string settingName,
                string settingValue)
                : base(settingName)
            {

            }
        }

        /// <summary>
        /// Settings that can be set for an azure table backed event stream
        /// </summary>
        /// <remarks>
        /// ImplementationType=AzureTable;ConnectionStringName;LeaguesConnectionString;SequenceNumberFormat=00000000
        /// </remarks>
        public class TableConnectionSettings
            : ConnectionSettingsBase
        {

            public const string DEFAULT_SEQUENCE_FORMAT = @"00000000";

            /// <summary>
            /// The format to use when storing the sequence number as a string
            /// </summary>
            /// <remarks>
            /// This is usually zero-padded to make indexing faster and sequence easier
            /// to read
            /// </remarks>
            private readonly string _sequenceNumberFormat;
            public string SequenceNumberFormat
            {
                get
                {
                    if (string.IsNullOrWhiteSpace(_sequenceNumberFormat))
                    {
                        return DEFAULT_SEQUENCE_FORMAT;
                    }
                    else
                    {
                        return _sequenceNumberFormat;
                    }
                }
            }

            public TableConnectionSettings(string settingName,
                    string settingValue)
                    : base(settingName)
            {

            }
        }
    }

    /// <summary>
    /// Properties shared by all the different types of connection settings
    /// </summary>
    public abstract class ConnectionSettingsBase
    {

        private string _settingType;
        /// <summary>
        /// The type of setting this pertains to 
        /// </summary>
        /// <remarks>
        /// EventStream or Snapshot
        /// </remarks>
        public string SettingType
        {
            get { return _settingType; }
        }

        private readonly string _mappedDomainName;
        /// <summary>
        /// The domain name the event stream entity belongs to
        /// </summary>
        public string MappedDomainName
        {
            get { return _mappedDomainName; }
        }

        private readonly string _mappedEntityType;
        /// <summary>
        /// The entity type that the event stream is mapped to
        /// </summary>
        public string MappedEntityTypeName
        {
            get
            {
                return _mappedEntityType;
            }
        }

        /// <summary>
        /// Create the settings base from the config property name(dot separated)
        /// </summary>
        /// <param name="settingName">
        /// The unique name of the setting e.g.
        /// EventSteam.Leagues.League
        /// Snapshot.Leagues.League.League_Summary_Information etc.
        /// </param>
        public ConnectionSettingsBase(string settingName)
        {
            if (!string.IsNullOrWhiteSpace(settingName))
            {
                string[] sections = settingName.Split('.');
                if (sections.Length > 0)
                {
                    _settingType = sections[0];
                }
                if (sections.Length > 1)
                {
                    _mappedDomainName = sections[1];
                }
                if (sections.Length > 2)
                {
                    _mappedEntityType = sections[2];
                }
            }
        }

    }
}
