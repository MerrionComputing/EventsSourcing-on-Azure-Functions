using EventSourcingOnAzureFunctions.Common.EventSourcing.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EventSourcingOnAzureFunctions.Common.Binding
{
    /// <summary>
    /// The information passed into an azure function that has been trigegred whenever a new entity is created
    /// </summary>
    public sealed class NewEntityTriggerData
        : IEventStreamIdentity
    {

        private readonly string _domainName;
        /// <summary>
        /// The domain in which this event stream is located
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
        /// The type of entity for which this event stream pertains
        /// </summary>
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
        public string InstanceKey
        {
            get
            {
                return _instanceKey;
            }
        }

        public NewEntityTriggerData(string domainName,
            string entityTypeName,
            string instanceKey,
            string notificationDispatcherName = "")
        {
            _domainName = domainName;
            _entityTypeName = entityTypeName;
            _instanceKey = instanceKey;
        }
    }
}
