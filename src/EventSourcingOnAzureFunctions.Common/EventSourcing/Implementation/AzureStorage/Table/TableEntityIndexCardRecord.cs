using EventSourcingOnAzureFunctions.Common.EventSourcing.Exceptions;
using EventSourcingOnAzureFunctions.Common.EventSourcing.Interfaces;
using Microsoft.Azure.Cosmos.Table;
using System;
using System.Collections.Generic;

namespace EventSourcingOnAzureFunctions.Common.EventSourcing.Implementation.AzureStorage.Table
{
    /// <summary>
    /// An index-card to allow rapid lookup of the entities in any domain/entity type
    /// </summary>
    /// <remarks>
    /// This is only used to speed up "all entities" queries as a cross-partition query is
    /// slower
    /// </remarks>
    public sealed class TableEntityIndexCardRecord
        : ITableEntity,
        IEventStreamIdentity
    {

        public const string INDEX_CARD_PARTITION = "INDEX-CARD";

        private string _domainName;
        /// <summary>
        /// The domain in which this event stream is set
        /// </summary>
        public string DomainName
        {
            get
            {
                return _domainName;
            }
            set
            {
                _domainName = value;
            }
        }

        private string _entityTypeName;
        /// <summary>
        /// The type of entity for which this event stream pertains
        /// </summary>
        public string EntityTypeName
        {
            get
            {
                return _entityTypeName;
            }
            set
            {
                _entityTypeName = value;
            }
        }

        private string _instanceKey;
        /// <summary>
        /// The specific uniquely identitified instance of the entity to which this event stream pertains
        /// </summary>
        public string InstanceKey
        {
            get
            {
                return _instanceKey;
            }
            set
            {
                _instanceKey = value;
            }
        }

        // Partition key holds the entity unique 
        public string PartitionKey
        {
            get
            {
                return INDEX_CARD_PARTITION;
            }
            set
            {
                if (!string.Equals(value, INDEX_CARD_PARTITION))
                {
                    throw new EventStreamReadException(this ,
                        0,
                        "Invalid index card record read");
                }
            }
        }

        // Row key is always zeroes - the base of the event stream
        public string RowKey
        {
            get
            {
                return InstanceKey;
            }
            set
            {
                _instanceKey = value;
            }
        }

        /// <summary>
        /// The time the record was last updated
        /// </summary>
        public DateTimeOffset Timestamp { get; set; }

        /// <summary>
        /// The special concurrency protection tag used to make sure no update has occured since the last read
        /// </summary>
        public string ETag { get; set; }


        public void ReadEntity(IDictionary<string, EntityProperty> properties,
          OperationContext operationContext)
        {
            if (null != properties)
            {
                if (properties.ContainsKey(nameof(DomainName)))
                {
                    DomainName = properties[nameof(DomainName)].StringValue;
                }
                if (properties.ContainsKey(nameof(EntityTypeName)))
                {
                    EntityTypeName = properties[nameof(EntityTypeName)].StringValue;
                }
                if (properties.ContainsKey(nameof(InstanceKey)))
                {
                    InstanceKey = properties[nameof(InstanceKey)].StringValue;
                }
            }
        }


        public IDictionary<string, EntityProperty> WriteEntity(OperationContext operationContext)
        {
            IDictionary<string, EntityProperty> ret = new Dictionary<string, EntityProperty>();
            // Add the custom properties here
            ret.Add(nameof(DomainName), EntityProperty.GeneratePropertyForString(DomainName));
            ret.Add(nameof(EntityTypeName), EntityProperty.GeneratePropertyForString(EntityTypeName));
            ret.Add(nameof(InstanceKey), EntityProperty.GeneratePropertyForString(InstanceKey));
            return ret;
        }
    }
}
