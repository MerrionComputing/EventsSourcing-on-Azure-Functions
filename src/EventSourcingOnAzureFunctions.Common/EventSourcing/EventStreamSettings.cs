using System;
using System.Collections.Generic;
using System.Text;

namespace EventSourcingOnAzureFunctions.Common.EventSourcing
{
    /// <summary>
    /// Settings in the [EventStreamSettings] section of the JSON settings file(s)
    /// </summary>
    public sealed class EventStreamSettings
    {
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
        public string DomainQualifiedEntityTypeName { get; }

        /// <summary>
        /// The type of storage implementation this event stream is based on
        /// </summary>
        public string Storage { get; }

        /// <summary>
        /// The connection string to use to access the underlying storage mechanism
        /// </summary>
        public string ConnectionStringName { get; }

        public EventStreamSetting(string domainQualifiedEntityTypeName,
            string storageType = @"",
            string connectionStringName = @"")
        {

        }

        /// <summary>
        /// The default settings to use if no settings are specified in the application configuration
        /// </summary>
        /// <param name="domainQualifiedEntityTypeName">
        /// The name of the type of entity that this event stream setting is for
        /// </param>
        public static EventStreamSetting DefaultEventStreamSetting(string domainQualifiedEntityTypeName)
        {
            return new EventStreamSetting(domainQualifiedEntityTypeName);
        }
    }
}
