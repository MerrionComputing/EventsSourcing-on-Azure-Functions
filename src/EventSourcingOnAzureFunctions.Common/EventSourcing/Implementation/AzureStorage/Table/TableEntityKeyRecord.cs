using EventSourcingOnAzureFunctions.Common.EventSourcing.Exceptions;
using EventSourcingOnAzureFunctions.Common.EventSourcing.Interfaces;
using Microsoft.Azure.Cosmos.Table;
using System;
using System.Collections.Generic;
using System.Text;

namespace EventSourcingOnAzureFunctions.Common.EventSourcing.Implementation.AzureStorage.Table
{
    /// <summary>
    /// The lowest record in an event stream that holds the stream meta data
    /// </summary>
    public sealed class TableEntityKeyRecord
        : ITableEntity,
        IEventStreamIdentity
    {

        private  string _domainName;
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

        private  string _entityTypeName;
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

        private  string _instanceKey;
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
                return InstanceKey;
            }
            set
            {
                _instanceKey  = value;
            }
        }

        // Row key is always zeroes - the base of the event stream
        public string RowKey
        {
            get
            {
                return TableEventStreamBase.RECORDID_SEQUENCE;
            }
            set
            {
                if (!string.Equals(value, TableEventStreamBase.RECORDID_SEQUENCE ) )
                {
                   // throw new EventStreamReadException();
                }
            }
        }


        public DateTimeOffset Timestamp { get; set; }

        /// <summary>
        /// The last sequence number for this event stream
        /// </summary>
        public int LastSequence { get; set; }

        /// <summary>
        /// Additional context information / commentary for this event stream
        /// </summary>
        public string Context { get; set; }

        /// <summary>
        /// This flag is set to indicate that an event stream is being deleted
        /// </summary>
        public bool Deleting { get; set; }

        /// <summary>
        /// The special concurrency protection tag used to make sure no update has occured since the last read
        /// </summary>
        public string ETag { get ; set; }

        public void ReadEntity(IDictionary<string, EntityProperty> properties,
            OperationContext operationContext)
        {
            if (null != properties )
            {
                if (properties.ContainsKey(nameof(LastSequence)) )
                {
                    LastSequence = properties[nameof(LastSequence)].Int32Value.GetValueOrDefault(0);
                }
                if (properties.ContainsKey(nameof(Context )) )
                {
                    Context = properties[nameof(Context)].StringValue;
                }
                if (properties.ContainsKey(nameof(DomainName) ))
                {
                    DomainName = properties[nameof(DomainName)].StringValue;
                }
                if (properties.ContainsKey(nameof(EntityTypeName)))
                {
                    EntityTypeName = properties[nameof(EntityTypeName)].StringValue;
                }
                if (properties.ContainsKey(nameof(InstanceKey )) )
                {
                    InstanceKey = properties[nameof(InstanceKey)].StringValue;
                }
                if (properties.ContainsKey(nameof(Deleting )))
                {
                    Deleting = properties[nameof(Deleting )].BooleanValue.GetValueOrDefault(false) ;
                }
            }
        }

        public IDictionary<string, EntityProperty> WriteEntity(OperationContext operationContext)
        {
            IDictionary<string, EntityProperty> ret = new Dictionary<string, EntityProperty>();
            // Add the custom properties here
            ret.Add(nameof(LastSequence), EntityProperty.GeneratePropertyForInt(LastSequence) );
            ret.Add(nameof(Context), EntityProperty.GeneratePropertyForString(Context));
            ret.Add(nameof(DomainName), EntityProperty.GeneratePropertyForString(DomainName));
            ret.Add(nameof(EntityTypeName), EntityProperty.GeneratePropertyForString(EntityTypeName));
            ret.Add(nameof(InstanceKey), EntityProperty.GeneratePropertyForString(InstanceKey));
            ret.Add(nameof(Deleting), EntityProperty.GeneratePropertyForBool(Deleting));
            return ret;
        }


        /// <summary>
        /// Parameter-less constructor for serialisation
        /// </summary>
        public TableEntityKeyRecord()
        {
        }

        public TableEntityKeyRecord(IEventStreamIdentity identity)
        {
            _domainName = identity.DomainName;
            _entityTypeName = identity.EntityTypeName;
            _instanceKey = identity.InstanceKey;
        }
    }
}
