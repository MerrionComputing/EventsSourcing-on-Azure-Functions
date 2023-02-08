using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using EventSourcingOnAzureFunctions.Common.Binding;
using EventSourcingOnAzureFunctions.Common.EventSourcing.Implementation.AzureStorage.AppendBlob;
using EventSourcingOnAzureFunctions.Common.EventSourcing.Implementation.AzureStorage.Table;
using EventSourcingOnAzureFunctions.Common.EventSourcing.Implementation.AzureStorage.File;
using EventSourcingOnAzureFunctions.Common.EventSourcing.Interfaces;
using Microsoft.Extensions.Configuration;

namespace EventSourcingOnAzureFunctions.Common.EventSourcing
{
    /// <summary>
    /// Settings in the [EventStreamSettings] section of the JSON settings file(s)
    /// </summary>
    public sealed class EventStreamSettings
        : IEventStreamSettings
    {

        private ConcurrentDictionary<string, EventStreamSetting> AllSettings = new ConcurrentDictionary<string, EventStreamSetting>();


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
                                AllSettings.TryAdd(setting.DomainQualifiedEntityTypeName, setting);
                            }
                        }
                    }
                }
            }
        }

        public void InitialiseEnvironmentStrings()
        {
            // Default load the .env files
            DotNetEnv.Env.Load();   
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
            CreateFronEnvironmentStringIfNotExists(attribute);
            if (AllSettings.ContainsKey(EventStreamSetting.MakeDomainQualifiedEntityName(attribute)))
            {
                return AllSettings[EventStreamSetting.MakeDomainQualifiedEntityName(attribute)].Storage.ToUpperInvariant()  ;
            }
            else
            {
                return DefaultEventStreamSetting(EventStreamSetting.MakeDomainQualifiedEntityName(attribute)).Storage.ToUpperInvariant();
            }
        }

        public string GetConnectionStringName(IEventStreamIdentity attribute)
        {
            CreateFronEnvironmentStringIfNotExists(attribute);
            if (AllSettings.ContainsKey(EventStreamSetting.MakeDomainQualifiedEntityName(attribute)))
            {
                return AllSettings[EventStreamSetting.MakeDomainQualifiedEntityName(attribute)].ConnectionStringName;
            }
            else
            {
                return ConnectionStringNameAttribute.DefaultConnectionStringName(attribute);
            }
        }

        private void CreateFronEnvironmentStringIfNotExists(IEventStreamIdentity attribute)
        {
            if (!AllSettings.ContainsKey(EventStreamSetting.MakeDomainQualifiedEntityName(attribute)))
            {
                string envValue = Environment.GetEnvironmentVariable(EventStreamSetting.MakeEnvironmentStringKey(attribute));
                if (string.IsNullOrWhiteSpace(envValue ) )
                {
                    envValue = Environment.GetEnvironmentVariable(EventStreamSetting.MakeEnvironmentStringKey(attribute, AllEntityTypes: true));
                }
                if (string.IsNullOrWhiteSpace(envValue))
                {
                    envValue = Environment.GetEnvironmentVariable(EventStreamSetting.MakeEnvironmentStringKey(attribute, AllDomains:  true));
                }
                if (string.IsNullOrWhiteSpace(envValue))
                {
                    envValue = Environment.GetEnvironmentVariable(EventStreamSetting.MakeEnvironmentStringKey(attribute, AllEntityTypes: true, AllDomains: true));
                }
                if (!string.IsNullOrWhiteSpace(envValue))
                {
                    EventStreamSetting newSetting = EventStreamSetting.SettingsFromEnvironmentStringValue(attribute, envValue);
                    if (null != newSetting)
                    {
                        AllSettings.TryAdd(EventStreamSetting.MakeDomainQualifiedEntityName(attribute),
                            newSetting);
                    }
                }
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

            if (GetBackingImplementationType(attribute ).Equals(EventStreamSetting.EVENTSTREAMIMPLEMENTATION_TABLE , StringComparison.OrdinalIgnoreCase  )  )
            {
                return new TableEventStreamWriter(attribute, connectionStringName: connectionStringName);
            }

            // Default to an appendblob writer AppendBlob
            return new BlobEventStreamWriter(attribute, connectionStringName: connectionStringName);
        }

        /// <summary>
        /// Create a projection processor to run over the given event stream's backing store
        /// </summary>
        public  IProjectionProcessor CreateProjectionProcessorForEventStream(ProjectionAttribute attribute)
        {
            string connectionStringName = GetConnectionStringName(attribute);

            if (GetBackingImplementationType(attribute).Equals(EventStreamSetting.EVENTSTREAMIMPLEMENTATION_TABLE , StringComparison.OrdinalIgnoreCase))
            {
                return TableEventStreamReader.CreateProjectionProcessor(attribute, connectionStringName: connectionStringName);
            }

            if (GetBackingImplementationType(attribute).Equals(EventStreamSetting.EVENTSTREAMIMPLEMENTATION_FILE , StringComparison.OrdinalIgnoreCase))
            {
                return FileEventStreamReader.CreateProjectionProcessor(attribute, connectionStringName: connectionStringName);
            }

            // Default to AppendBlob
            return BlobEventStreamReader.CreateProjectionProcessor(attribute, connectionStringName);
        }

        /// <summary>
        /// Create a classification processor to run over the given event stream's backing store
        /// </summary>
        public IClassificationProcessor CreateClassificationProcessorForEventStream(ClassificationAttribute attribute)
        {
            string connectionStringName = GetConnectionStringName(attribute);

            if (GetBackingImplementationType(attribute).Equals(EventStreamSetting.EVENTSTREAMIMPLEMENTATION_TABLE, StringComparison.OrdinalIgnoreCase))
            {
                return TableEventStreamReader.CreateClassificationProcessor(attribute, connectionStringName: connectionStringName);
            }

            // Default to AppendBlob
            return BlobEventStreamReader.CreateClassificationProcessor(attribute, connectionStringName);
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
        public static string EVENTSTREAMIMPLEMENTATION_TABLE = @"Table";
        public static string EVENTSTREAMIMPLEMENTATION_FILE = @"File";

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
            // Default storage to Table
            Storage = EVENTSTREAMIMPLEMENTATION_TABLE ;
        }





        /// <summary>
        /// Get the domain qualified entity name for an event stream
        /// </summary>
        /// <param name="eventStreamIdentity">
        /// The unique identity of the event stream
        /// </param>
        public static string MakeDomainQualifiedEntityName(IEventStreamIdentity eventStreamIdentity,
            bool AllEntityTypes = false ,
            bool AllDomains = false )
        {
            if (null != eventStreamIdentity)
            {
                if (! (AllEntityTypes || AllEntityTypes))
                { 
                    return $"{eventStreamIdentity.DomainName}.{eventStreamIdentity.EntityTypeName}";
                }
                if (AllEntityTypes && AllDomains )
                {
                    return @"ALL.ALL";
                }
                if (AllEntityTypes )
                {
                    return $"{eventStreamIdentity.DomainName}.ALL";
                }
                if (AllDomains )
                {
                    return $"ALL.{eventStreamIdentity.EntityTypeName}";
                }
            }
            return @"";
        }

        /// <summary>
        /// Get the environment string key that can be used to set the event stream settings for the given domain and entity type
        /// </summary>
        /// <param name="eventStreamIdentity">
        /// The identity of the domain and entity type that has its history stored in the given event stream
        /// </param>
        public static string MakeEnvironmentStringKey(IEventStreamIdentity eventStreamIdentity,
            bool AllEntityTypes = false,
            bool AllDomains = false)
        {
            string ret = MakeDomainQualifiedEntityName(eventStreamIdentity,
                AllEntityTypes ,
                AllDomains);

            // Make it a valid environment string - must not contain = and max length is 255
            ret = ret.Replace("=", "_");
            if (ret.Length > 255)
            {
                int magicNumber = GetStableHashCode(ret);
                ret = ret.Substring(0, 240) + magicNumber.ToString("0000000000");   
            }
            return ret;
        }

        public static EventStreamSetting SettingsFromEnvironmentStringValue(IEventStreamIdentity eventStreamIdentity,
            string environmentStringValue)
        {
            EventStreamSetting ret = new EventStreamSetting(MakeDomainQualifiedEntityName(eventStreamIdentity ));
            if (! string.IsNullOrWhiteSpace(environmentStringValue ) )
            {
                if (environmentStringValue.StartsWith("Table;")  )
                {
                    ret.Storage = EVENTSTREAMIMPLEMENTATION_TABLE; 
                    if (environmentStringValue.Length > @"Table;".Length  )
                    {
                        ret.ConnectionStringName = environmentStringValue.Substring(6);  
                    }
                }
                if (environmentStringValue.StartsWith("AppendBlob;"))
                {
                    ret.Storage = EVENTSTREAMIMPLEMENTATION_APPENDBLOB ;
                    if (environmentStringValue.Length > @"AppendBlob;".Length)
                    {
                        ret.ConnectionStringName = environmentStringValue.Substring(11);
                    }
                }
                if (environmentStringValue.StartsWith("File;"))
                {
                    ret.Storage = EVENTSTREAMIMPLEMENTATION_FILE;
                    if (environmentStringValue.Length > @"File;".Length)
                    {
                        ret.ConnectionStringName = environmentStringValue.Substring(5);
                    }
                }
            }
            // If not able to make a setting from the environment string, fall back on the default
            if (string.IsNullOrWhiteSpace(ret.Storage )  )
            {
                // Default  to event blob storage
                ret.Storage = EVENTSTREAMIMPLEMENTATION_APPENDBLOB;
            }
            if (string.IsNullOrWhiteSpace(ret.ConnectionStringName ) )
            {
                ret.ConnectionStringName=ConnectionStringNameAttribute.DefaultConnectionStringName(eventStreamIdentity);  
            }

            return ret;
        }

        public static int GetStableHashCode( string str)
        {
            unchecked
            {
                int hash1 = 5381;
                int hash2 = hash1;

                for (int i = 0; i < str.Length && str[i] != '\0'; i += 2)
                {
                    hash1 = ((hash1 << 5) + hash1) ^ str[i];
                    if (i == str.Length - 1 || str[i + 1] == '\0')
                        break;
                    hash2 = ((hash2 << 5) + hash2) ^ str[i + 1];
                }

                return hash1 + (hash2 * 1566083941);
            }
        }
    }
}
