using EventSourcingOnAzureFunctions.Common.Binding;
using EventSourcingOnAzureFunctions.Common.EventSourcing.Implementation.AzureStorage.AppendBlob;
using EventSourcingOnAzureFunctions.Common.EventSourcing.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace EventSourcingOnAzureFunctions.Common.EventSourcing
{
    /// <summary>
    /// Top levekl access to the event stream for an entity
    /// </summary>
    public class EventStream
        : IEventStreamIdentity
    {

        private readonly IEventStreamWriter _writer = null;
        private string _connectionStringName;

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

        /// <summary>
        /// Does the event stream identified by the domain/type/instance exist yet
        /// </summary>
        public async Task<bool> Exists()
        {
            if (null != _writer)
            {
                return await _writer.Exists();
            }
            else
            {
                // It cannot exist if the event stream writer was not created
                return false;
            }
        }


        private IWriteContext _context = null;
        /// <summary>
        /// The writer context which is used to "wrap" events written so we know who wrote them (and why)
        /// </summary>
        public IWriteContext Context
        {
            get
            {
                return _context;
            }
        }

        /// <summary>
        /// Set the writer context which is used to "wrap" events written so we know who wrote them (and why)
        /// </summary>
        /// <param name="context">
        /// The context to use when writing events
        /// </param>
        public void SetContext(IWriteContext context)
        {
            if (null != context)
            {
                _context = context;
                if (null != _writer)
                {
                    _writer.SetContext(_context);
                }
            }
        }


        public override string ToString()
        {
            return $"EventStream({DomainName}::{EntityTypeName }::{InstanceKey })";
        }


        public EventStream(EventStreamAttribute attribute,
            string connectionStringName = "",
            IWriteContext context = null)
        {
            _domainName = attribute.DomainName;
            _entityTypeName  = attribute.EntityTypeName ;
            _instanceKey  = attribute.InstanceKey;

            if (string.IsNullOrWhiteSpace(connectionStringName))
            {
                _connectionStringName = ConnectionStringNameAttribute.DefaultConnectionStringName(attribute);
            }
            else
            {
                _connectionStringName = connectionStringName;
            }

            // wire up the event stream writer 
            // TODO : Cater for different backing technologies... currently just AppendBlob
            _writer = new BlobEventStreamWriter(attribute,  connectionStringName:_connectionStringName);

            if (null != context)
            {
                _context = context;
                if (null != _writer)
                {
                    _writer.SetContext(_context);
                }
            }

        }
    }
}
