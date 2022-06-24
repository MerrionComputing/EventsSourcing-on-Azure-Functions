using EventSourcingOnAzureFunctions.Common.EventSourcing.Exceptions;
using EventSourcingOnAzureFunctions.Common.EventSourcing.Interfaces;
using Azure.Data.Tables;
using System;
using System.Collections.Generic;

namespace EventSourcingOnAzureFunctions.Common.EventSourcing.Implementation.AzureStorage.Table
{
    public sealed class TableClassificationRecord
        : ITableEntity, 
        IEventStreamIdentity
    {

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

        private int _asOfSequence;
        /// <summary>
        /// The event sequence number as of which the snapshot was taken
        /// </summary>
        /// <remarks>
        /// This is stored in the record row key
        /// </remarks>
        public int AsOfSequence
        {
            get
            {
                return _asOfSequence;
            }
            set
            {
                _asOfSequence = value;
            }
        }


        /// <summary>
        /// Was this entity ever included in this classification
        /// </summary>
        public bool WasEverIncluded { get; set; }

        /// <summary>
        /// Was this entity ever excluded from this classification
        /// </summary>
        public bool WasEverExcluded { get; set; }

        /// <summary>
        /// The classification state as at the point the snapshot is taken
        /// </summary>
        public ClassificationResponse.ClassificationResults CurrentClassification { get; set; }

        // Partition key holds the entity unique identifier
        public string PartitionKey
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
        /// Row key holds the as-of sequence number
        /// </summary>
        public string RowKey
        {
            get
            {
               return TableEventStreamBase.SequenceNumberAsString (_asOfSequence );
            }
            set
            {
                _asOfSequence = TableEventStreamBase.SequenceNumberFromString(value);
            }
        }

        public DateTimeOffset? Timestamp { get; set; }


        /// <summary>
        /// The special concurrency protection tag used to make sure no update has occured since the last read
        /// </summary>
        public Azure.ETag ETag { get; set; }


    }
}
