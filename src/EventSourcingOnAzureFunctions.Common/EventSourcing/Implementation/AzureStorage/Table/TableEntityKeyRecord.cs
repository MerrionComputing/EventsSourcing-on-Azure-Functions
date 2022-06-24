using EventSourcingOnAzureFunctions.Common.EventSourcing.Exceptions;
using EventSourcingOnAzureFunctions.Common.EventSourcing.Interfaces;
using Azure.Data.Tables;
using System;
using System.Collections.Generic;

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
                   throw new EventStreamReadException(this,
                       0, 
                       "Invalid record sequence for key record");
                }
            }
        }


        public DateTimeOffset? Timestamp { get; set; }

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
        public Azure.ETag  ETag { get ; set; }


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
