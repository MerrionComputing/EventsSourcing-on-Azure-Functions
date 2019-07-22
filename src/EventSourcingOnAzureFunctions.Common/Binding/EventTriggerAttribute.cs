using System;
using System.Collections.Generic;
using System.Text;

namespace EventSourcingOnAzureFunctions.Common.Binding
{
    /// <summary>
    /// Attribute to be fired whenever a particluar event is written to a domain event stream
    /// </summary>
    [AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false)]
    public sealed class EventTriggerAttribute
        : Attribute 
    {


        public static string ALL_EVENT_TYPES = @"*";  // Indicates we want to respond to all event types on the domain/entity
        public static string ALL_INSTANCE_KEYS = @"*"; // Indicates we want to respond to events for all instance keys on the domain/entity

        private readonly string _domainName;
        /// <summary>
        /// The domain in which the event stream being monitored is located
        /// </summary>
        public string DomainName
        {
            get
            {
                return _domainName;
            }
        }

        private readonly string _entityTypeName;
        /// <summary>
        /// The type of entity for which the event stream being monitored pertains
        /// </summary>
        public string EntityTypeName
        {
            get
            {
                return _entityTypeName;
            }
        }

        /// <summary>
        /// The filter to use to decide what event(s) to respond to
        /// </summary>
        /// <remarks>
        /// This can be set to ALL_EVENT_TYPES to respond regardless of the event type
        /// </remarks>
        public string EventNameFilter { get; }

        /// <summary>
        /// The filter to use to decide what instance key(s) to respond to
        /// </summary>
        /// <remarks>
        /// This can be set to ALL_INSTANCE_KEYS to respond regardless of the instance key
        /// </remarks>
        public string InstanceKeyFilter { get; }


        public EventTriggerAttribute(string DomainNameSource,
            string EntityTypeNameSource,
            string EventNameFilterToUse = @""  ,
            string InstanceKeyFilterToUse = @"" )
        {
            // Populate the attribute parameters
            _domainName = DomainNameSource;
            _entityTypeName = EntityTypeNameSource;
            if (string.IsNullOrEmpty(EventNameFilterToUse ) )
            {
                EventNameFilter = ALL_EVENT_TYPES;
            }
            else
            {
                EventNameFilter = EventNameFilterToUse;
            }
            if (string.IsNullOrEmpty(InstanceKeyFilterToUse ) )
            {
                InstanceKeyFilter = ALL_INSTANCE_KEYS;
            }
            else
            {
                InstanceKeyFilter = InstanceKeyFilterToUse;
            }
        }
    }
}
