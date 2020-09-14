using EventSourcingOnAzureFunctions.Common.Binding;
using EventSourcingOnAzureFunctions.Common.EventSourcing.Implementation;
using EventSourcingOnAzureFunctions.Common.EventSourcing.Implementation.AzureStorage.AppendBlob;
using EventSourcingOnAzureFunctions.Common.EventSourcing.Interfaces;
using EventSourcingOnAzureFunctions.Common.Notification;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using static EventSourcingOnAzureFunctions.Common.EventSourcing.Implementation.EventStreamBase;

namespace EventSourcingOnAzureFunctions.Common.EventSourcing
{
    /// <summary>
    /// Top level access to the event stream for an entity
    /// </summary>
    public class EventStream
        : IEventStreamIdentity
    {

        private readonly INotificationDispatcher _notificationDispatcher = null;
        private readonly IEventStreamSettings _settings = null;
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

        /// <summary>
        /// Append an event onto the event stream 
        /// </summary>
        /// <param name="eventToAppend">
        /// The event we want to write to the event stream
        /// </param>
        /// <param name="expectedTopSequence">
        /// The sequence number we expect to be the last in the event stream - if events have been added since this then a 
        /// concurrency error is thrown
        /// </param>
        public async Task AppendEvent(object eventToAppend, 
            int? expectedTopSequence = null,
            EventStreamExistenceConstraint streamConstraint = EventStreamExistenceConstraint.Loose)
        {
            if (null != _writer )
            {
                // make an event instance of this event and append it to the event stream
               IAppendResult result= await _writer.AppendEvent(EventInstance.Wrap(eventToAppend), 
                   expectedTopSequence.GetValueOrDefault(0), 
                    streamConstraint:streamConstraint ); 

                if (null != result )
                {
                    if (null != this._notificationDispatcher )
                    {
                        if (result.NewEventStreamCreated  )
                        {
                           await _notificationDispatcher.NewEntityCreated(this,
                               commentary: _context?.Commentary,
                               context: _context  ); 
                        }
                        await _notificationDispatcher.NewEventAppended(this, 
                            EventNameAttribute.GetEventName(eventToAppend.GetType()), 
                            result.SequenceNumber,
                            commentary: _context?.Commentary,
                            eventPayload: eventToAppend,
                            context: _context  ); 
                    }
                }
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

        /// <summary>
        /// Delete this event stream instance
        /// </summary>
        /// <remarks>
        /// This is included for short-lived entities (like commands, queries, sagas) but is 
        /// not a good idea for real business entities.
        /// </remarks>
        public async Task DeleteStream()
        {
            if (null != _writer)
            {
                _writer.DeleteStream();
                // Send a notification that this has occured
                if (null != this._notificationDispatcher)
                {
                    await this._notificationDispatcher.ExistingEntityDeleted(this,
                            commentary: _context?.Commentary,
                            context: _context); 
                }
            }
        }

        public async Task WriteIndex()
        {
            if (null != _writer )
            {
                await _writer.WriteIndex(); 
            }
        }

        public override string ToString()
        {
            return $"EventStream({DomainName}::{EntityTypeName }::{InstanceKey })";
        }


        public EventStream(EventStreamAttribute attribute,
            IWriteContext context = null,
            IEventStreamSettings settings = null,
            INotificationDispatcher dispatcher = null
            )
        {
            _domainName = attribute.DomainName;
            _entityTypeName  = attribute.EntityTypeName ;
            _instanceKey  = attribute.InstanceKey;


            if (null == settings)
            {
                _settings = new EventStreamSettings();
            }
            else
            {
                _settings = settings;
            }

            _connectionStringName = _settings.GetConnectionStringName(attribute);  

            // wire up the event stream writer 
            _writer = _settings.CreateWriterForEventStream(attribute);

            if (null != context)
            {
                _context = context;
                if (null != _writer)
                {
                    _writer.SetContext(_context);
                }
            }

            if (null == dispatcher)
            {
                // Create a new dispatcher 
                _notificationDispatcher = NotificationDispatcherFactory.NotificationDispatcher; 
            }
            else
            {
                _notificationDispatcher = dispatcher;
            }

        }
    }
}
