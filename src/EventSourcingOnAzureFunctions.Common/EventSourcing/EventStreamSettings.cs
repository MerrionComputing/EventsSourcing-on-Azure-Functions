using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using EventSourcingOnAzureFunctions.Common.Binding;
using EventSourcingOnAzureFunctions.Common.EventSourcing.Implementation.AzureStorage.AppendBlob;
using EventSourcingOnAzureFunctions.Common.EventSourcing.Implementation.AzureStorage.Table;
using EventSourcingOnAzureFunctions.Common.EventSourcing.Interfaces;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Configuration;

namespace EventSourcingOnAzureFunctions.Common.EventSourcing
{
    /// <summary>
    /// Settings in the [EventStreamSettings] section of the JSON settings file(s)
    /// </summary>
    public sealed class EventStreamSettings
        : IEventStreamSettings
    {

        private Dictionary<string, EventStreamSetting> AllSettings = new Dictionary<string, EventStreamSetting>();


        public void LoadFromConfig(string basePath = null)
        {

            if (string.IsNullOrWhiteSpace(basePath))
            {
                basePath  = Environment.GetEnvironmentVariable("AzureWebJobsScriptRoot")  // local_root
                    ?? (Environment.GetEnvironmentVariable("HOME") == null
                        ? Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)
                        : $"{Environment.GetEnvironmentVariable("HOME")}/site/wwwroot"); // azure_root
            }


            ConfigurationBuilder builder = new ConfigurationBuilder();
            if (!string.IsNullOrWhiteSpace(basePath))
            {
                builder.SetBasePath(basePath);
            }

            builder.AddJsonFile("appsettings.json", true)
                .AddJsonFile("config.local.json", true)
                .AddJsonFile("config.json", true)
                .AddJsonFile("connectionstrings.json", true)
                .AddEnvironmentVariables();

            IConfigurationRoot config = builder.Build();

            // Get the [EventStreamSettings] section
            IConfigurationSection eventStreamSettingSection = config.GetSection("EventStreamSettings");
            if (null != eventStreamSettingSection)
            {
                var configStreamSettings = eventStreamSettingSection.Get<List<EventStreamSetting>>(c => c.BindNonPublicProperties = true);

                if (null != configStreamSettings)
                {
                    foreach (EventStreamSetting setting in configStreamSettings)
                    {
                        if (null != setting)
                        {
                            if (!AllSettings.ContainsKey(setting.DomainQualifiedEntityTypeName))
                            {
                                AllSettings.Add(setting.DomainQualifiedEntityTypeName, setting);
                            }
                        }
                    }
                }
            }
        }


        /// <summary>
        /// The default settings to use if no settings are specified in the application configuration
        /// </summary>
        /// <param name="domainQualifiedEntityTypeName">
        /// The name of the type of entity that this event stream setting is for
        /// </param>
        public EventStreamSetting DefaultEventStreamSetting(string domainQualifiedEntityTypeName)
        {
            if (AllSettings.ContainsKey(domainQualifiedEntityTypeName ) )
            {
                return AllSettings[domainQualifiedEntityTypeName]; 
            }
            return new EventStreamSetting(domainQualifiedEntityTypeName);
        }

        public string GetBackingImplementationType(IEventStreamIdentity attribute)
        {
            return DefaultEventStreamSetting(EventStreamSetting.MakeDomainQualifiedEntityName(attribute)).Storage.ToUpperInvariant() ; 
        }

        public string GetConnectionStringName(IEventStreamIdentity attribute)
        {
            if (AllSettings.ContainsKey(EventStreamSetting.MakeDomainQualifiedEntityName(attribute)))
            {
                return AllSettings[EventStreamSetting.MakeDomainQualifiedEntityName(attribute)].ConnectionStringName ;
            }
            else
            {
                return ConnectionStringNameAttribute.DefaultConnectionStringName(attribute);
            }
        }

        /// <summary>
        /// Create an instance of the appropriate event stream writer to use for the given event stream
        /// </summary>
        /// <param name="attribute">
        /// The attribute defining the Domain,Entity Type and Instance Key of the event stream
        /// </param>
        /// <remarks>
        /// This is to allow different event streams to be held in different backing technologies
        /// </remarks>
        public IEventStreamWriter CreateWriterForEventStream(IEventStreamIdentity attribute)
        {
            string connectionStringName = GetConnectionStringName(attribute); 

            if (GetBackingImplementationType(attribute ).Equals(EventStreamSetting.EVENTSTREAMIMPLEMENTATIOIN_TABLE , StringComparison.OrdinalIgnoreCase  )  )
            {
                return new TableEventStreamWriter(attribute, connectionStringName: connectionStringName);
            }

            // Default to an appendblob writer AppendBlob
            return new BlobEventStreamWriter(attribute, connectionStringName: connectionStringName);
        }

        /// <summary>
        /// Create a projection processor to run over the given event stream's backing store
        /// </summary>
        /// <param name="attribute">
        /// </param>
        /// <param name="connectionStringName">
        /// </param>
        /// <remarks>
        /// 
        /// </remarks>
        public  IProjectionProcessor CreateProjectionProcessorForEventStream(ProjectionAttribute attribute)
        {
            string connectionStringName = GetConnectionStringName(attribute);

            if (GetBackingImplementationType(attribute).Equals(EventStreamSetting.EVENTSTREAMIMPLEMENTATIOIN_TABLE, StringComparison.OrdinalIgnoreCase))
            {
                return TableEventStreamReader.CreateProjectionProcessor(attribute, connectionStringName: connectionStringName);
            }

            // Default to AppendBlob
            return BlobEventStreamReader.CreateProjectionProcessor(attribute, connectionStringName);
        }


    }


    /// <summary>
    /// The settings to use for a given event stream
    /// </summary>
    /// <remarks>
    /// This is used to set where/how the underlying data of the event stream are stored.
    /// If not set by a configuration setting the default is used
    /// </remarks>
    public sealed class EventStreamSetting
    {

        public static string EVENTSTREAMIMPLEMENTATION_APPENDBLOB = @"AppendBlob";
        public static string EVENTSTREAMIMPLEMENTATIOIN_TABLE = @"Table";

        /// <summary>
        /// The name of the type of entity that this event stream setting is for
        /// </summary>
        /// <remarks>
        /// This is in the form of a dot-separated name
        /// </remarks>
        public string DomainQualifiedEntityTypeName { get; internal set; }

        /// <summary>
        /// The type of storage implementation this event stream is based on
        /// </summary>
        public string Storage { get; internal set; }

        /// <summary>
        /// The connection string to use to access the underlying storage mechanism
        /// </summary>
        public string ConnectionStringName { get; internal set; }

        public EventStreamSetting(string domainQualifiedEntityTypeName,
            string storageType = @"",
            string connectionStringName = @"")
        {
            Storage = storageType;
            ConnectionStringName = connectionStringName;
        }

        public EventStreamSetting()
        {
            // Default storage to append blob
            Storage = EVENTSTREAMIMPLEMENTATION_APPENDBLOB;
        }





        /// <summary>
        /// Get the domain qualified entity name for an event stream
        /// </summary>
        /// <param name="eventStreamIdentity">
        /// The unique identity of the event stream
        /// </param>
        public static string MakeDomainQualifiedEntityName(IEventStreamIdentity eventStreamIdentity)
        {
            if (null != eventStreamIdentity)
            {
                return $"{eventStreamIdentity.DomainName}.{eventStreamIdentity.EntityTypeName}";
            }
            return @"";
        }
    }
}
