using System;
using EventSourcingOnAzureFunctions.Common.EventSourcing.Interfaces;
using Microsoft.Azure.WebJobs.Description;

namespace EventSourcingOnAzureFunctions.Common.Binding
{
    /// <summary>
    /// Attribute to be fired whenever an event stream is created for a new entity
    /// </summary>
    /// <remarks>
    /// This will only apply to functions in the same function app - all intra domain communtication
    /// should be performed by Azure EventGrid
    /// </remarks>
    [AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false)]
    [Binding]
    public sealed class NewEntityTriggerAttribute
        : Attribute
    {

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
        /// The filter to use to decide what instance key(s) to respond to
        /// </summary>
        /// <remarks>
        /// This can be set to ALL_INSTANCE_KEYS to respond regardless of the instance key
        /// </remarks>
        public string InstanceKeyFilter { get; }


        public NewEntityTriggerAttribute(string DomainNameSource,
                string EntityTypeNameSource,
                string InstanceKeyFilterToUse = @"")
        {
            // Populate the attribute parameters
            _domainName = DomainNameSource;
            _entityTypeName = EntityTypeNameSource;
            if (string.IsNullOrEmpty(InstanceKeyFilterToUse))
            {
                InstanceKeyFilter = EventTriggerAttribute.ALL_INSTANCE_KEYS;
            }
            else
            {
                InstanceKeyFilter = InstanceKeyFilterToUse;
            }
        }
    }
}
