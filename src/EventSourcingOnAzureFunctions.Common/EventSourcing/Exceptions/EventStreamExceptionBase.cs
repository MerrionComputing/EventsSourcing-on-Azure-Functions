using EventSourcingOnAzureFunctions.Common.EventSourcing.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace EventSourcingOnAzureFunctions.Common.EventSourcing.Exceptions
{
    public class EventStreamExceptionBase
        : Exception , IEventStreamIdentity
    {

        private readonly string _domainName;
        /// <summary>
        /// The domain in which this event stream exception occured
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
        /// The type of entity for which this event stream error pertains
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

        private readonly int _sequenceNumber;
        public int SequenceNumber
        {
            get
            {
                return _sequenceNumber;
            }
        }

        public EventStreamExceptionBase(IEventStreamIdentity eventStreamIdentity,
            int sequenceNumber,
            string message = "",
            Exception innerException = null,
            string source = "")
            : base(message, innerException )
        {

            _sequenceNumber = sequenceNumber;
            if (null != eventStreamIdentity)
            {
                _domainName = eventStreamIdentity.DomainName;
                _entityTypeName = eventStreamIdentity.EntityTypeName;
                _instanceKey = eventStreamIdentity.InstanceKey;
            }

        }

    }
}
