using System;
using System.Threading.Tasks;
using EventSourcingOnAzureFunctions.Common.Binding;
using EventSourcingOnAzureFunctions.Common.EventSourcing;
using Microsoft.Azure.WebJobs.Host.Bindings;
using Microsoft.Azure.WebJobs.Host.Config;
using Microsoft.Extensions.DependencyInjection;

namespace EventSourcingOnAzureFunctions.Common
{

    /// <summary>
    /// Hard coded bindings to use for the different event sourcing domains
    /// </summary>
    /// <remarks>
    /// This could be set to use a config file in a production system that we wanted to have
    /// [Dev|QA|UAT|Prod] environments
    /// </remarks>
    public static class CQRSAzureBindings
    {

        /// <summary>
        /// The backing store types that are supported by the CQRS on EventGrid framework
        /// </summary>
        /// <remarks>
        /// This is a subset of the CQRSAzure library methods as some don't fit well with the
        /// EventGrid architecture
        /// </remarks>
        public enum BackingStoreType
        {
            /// <summary>
            /// (Default) Store the event streams in an AppendBlob 
            /// </summary>
            /// <remarks>
            /// ImplementationType="AzureBlob"
            /// ConnectionStringName="LeaguesConnectionString" 
            /// DomainName="Leagues"
            /// </remarks>
            AppendBlob = 0,
            /// <summary>
            /// Store the event streams in Azure table storage
            /// </summary>
            /// <remarks>
            /// ImplementationType="AzureTable"
            /// ConnectionStringName="LeaguesConnectionString" 
            /// SequenceNumberFormat="00000000"
            /// </remarks>
            Table = 1,
            /// <summary>
            /// Store the event streams in an Azure file
            /// </summary>
            /// <remarks>
            /// ImplementationType="AzureFile"
            /// ConnectionStringName="LeaguesConnectionString" 
            /// InitialSize="20000" 
            /// </remarks>
            File = 2
        }

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


        }

        /// <summary>
        /// Initailise any common dependency injection configuration settings
        /// </summary>
        /// <param name="context"></param>
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

            // If possible get the connection string to use

            // If possible, get the write context to use

            // Use this and the attribute to create a new event stream instance
            return Task<EventStream>.FromResult(new EventStream(attribute));
        }



        public static Task<Projection> BuildProjectionFromAttribute(ProjectionAttribute attribute,
            ValueBindingContext context)
        {
            // If possible get the connection string to use

            // Use this and the attribute to create a new classifier instance
            return Task<Projection>.FromResult(new Projection(attribute));
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


        /// <summary>
        /// Settings that can be set for a file backed event stream
        /// </summary>
        /// <remarks>
        /// ImplementationType=AzureFile;ConnectionStringName=LeaguesConnectionString;InitialSize=20000 
        /// </remarks>
        public class FileConnectionSettings
        {

            private const int DEFAULT_INITIAL_SIZE = 20000;

            /// <summary>
            /// The initial size to create the event stream file
            /// </summary>
            private string _initialSize;
            public int InitialSize
            {
                get
                {
                    if (!string.IsNullOrWhiteSpace(_initialSize))
                    {
                        int ret;
                        if (int.TryParse(_initialSize, out ret))
                        {
                            if (ret > 0)
                            {
                                return ret;
                            }
                        }
                    }
                    return DEFAULT_INITIAL_SIZE;
                }
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
