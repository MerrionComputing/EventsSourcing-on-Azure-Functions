using System;

using EventSourcingOnAzureFunctions.Common.EventSourcing.Interfaces;
using Microsoft.Azure.WebJobs.Description;

namespace EventSourcingOnAzureFunctions.Common.Binding
{
    /// <summary>
    /// An attribute to mark an event stream to use for output for appending events to
    /// </summary>
    [AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false)]
    [Binding]
    public sealed class EventStreamAttribute
        : Attribute , IEventStreamIdentity
    {

        private readonly string _domainName;
        /// <summary>
        /// The domain in which this event stream is located
        /// </summary>
        [AutoResolve]
        public string DomainName
        {
            get
            {
                return _domainName;
            }
        }


        private readonly string _entityTypeName;
        /// <summary>
        /// The type of entity for which this event stream pertains
        /// </summary>
        [AutoResolve]
        public string EntityTypeName
        {
            get
            {
                return _entityTypeName;
            }
        }

        private readonly string _instanceKey;
        /// <summary>
        /// The specific uniquely identitified instance of the entity to which this event stream pertains
        /// </summary>
        [AutoResolve]
        public string InstanceKey
        {
            get
            {
                return _instanceKey;
            }
        }

        private readonly string _notificationDispatcherName;
        [AutoResolve ]
        public string NotificationDispatcherName
        {
            get
            {
                return _notificationDispatcherName;
            }
        }

        // Note: The parameter names need to match the property names (except for the camelCase) because the autoresolve
        // uses this fact to perform the instantiation
        public EventStreamAttribute(string domainName,
            string entityTypeName,
            string instanceKey,
            string notificationDispatcherName = "")
        {
            _domainName = domainName;
            _entityTypeName = entityTypeName;
            _instanceKey = instanceKey;
            _notificationDispatcherName = notificationDispatcherName;
        }

    }
}
