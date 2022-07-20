using EventSourcingOnAzureFunctions.Common.EventSourcing.Interfaces;
using Azure.Data.Tables;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace EventSourcingOnAzureFunctions.Common.EventSourcing.Implementation.AzureStorage.Table
{
    public sealed class TableEventStreamReader
        : TableEventStreamBase, 
        IEventStreamReader
    {

        private readonly IEventMaps _eventMaps = null;
        
        public async Task<IEnumerable<IEvent>> GetAllEvents()
        {
            // Get all the events starting from the start of the stream
            return await GetEvents(1); 
        }

        public async Task<IEnumerable<IEvent>> GetEvents(int StartingSequenceNumber = 1, 
            DateTime? effectiveDateTime = null)
        {
            if (StartingSequenceNumber < 1)
            {
                StartingSequenceNumber = 1;
            }

            if (Table != null)
            {
                List<IEvent> ret = new List<IEvent>();

                Azure.Pageable<TableEntity> getEventsQuery = Table.Query<TableEntity>(filter: CreateQuery(StartingSequenceNumber));

                foreach (var entity in getEventsQuery)
                {
                    // Get the event data
                    IEvent eventPayload = GetEventFromDynamicTableEntity(entity);
                    if (null != eventPayload)
                    {
                        // Add it to the end of the list
                        ret.Add(eventPayload);
                    }
                }

                // return the resultant list
                return ret;
            }
            else
            {
                // Initialisation error with the event stream
                throw new Exceptions.EventStreamReadException(this,
                    0,
                    "Event stream initialisation error",
                    source: "Azure Tables event stream reader");
            }
        }


        public async Task<IEnumerable<IEventContext>> GetEventsWithContext(int StartingSequenceNumber = 1, 
            DateTime? effectiveDateTime = null)
        {
            if (StartingSequenceNumber < 1)
            {
                StartingSequenceNumber = 1;
            }

            if (Table != null)
            {
                List<IEventContext> ret = new List<IEventContext>();

                Azure.Pageable<TableEntity> getEventsQuery = Table.Query<TableEntity>(filter: CreateQuery(StartingSequenceNumber));

                foreach (var entity in getEventsQuery)
                {
                    // Get the event data
                    IEvent eventPayload = GetEventFromDynamicTableEntity(entity);
                    if (null != eventPayload)
                    {
                        // Wrap it in the event context
                        IEventContext wrappedEvent = WrapEventFromDynamicTableEntity(entity, eventPayload);
                        if (null != wrappedEvent)
                        {
                            if ((!effectiveDateTime.HasValue) || (wrappedEvent.EventWrittenDateTime <= effectiveDateTime.Value))
                            {
                                // Add it to the end of the list
                                ret.Add(wrappedEvent);
                            }
                        }
                    }
                }

                // return the resultant list
                return ret;
            }
            else
            {
                // Initialisation error with the event stream
                throw new Exceptions.EventStreamReadException(this,
                    0,
                    "Event stream initialisation error",
                    source: "Azure Tables event stream reader");  
            }

        }

        private IEventContext WrapEventFromDynamicTableEntity(TableEntity dteRow, 
            IEvent eventPayload)
        {
            if (null != eventPayload )
            {
                return new TableContextWrappedEvent(eventPayload, dteRow);
            }

            // Not enough data to make a wrapped event
            return null;
        }


        private IEvent GetEventFromDynamicTableEntity(TableEntity dteRow)
        {
            if (null != dteRow )
            {
                // get the event name
                string eventName = null;

                if (dteRow.ContainsKey(FIELDNAME_EVENTTYPE))
                {
                    eventName = dteRow.GetString(FIELDNAME_EVENTTYPE);
                }

                if (!string.IsNullOrWhiteSpace(eventName))
                {
                    IEvent eventWrapper = CreateEventClass(eventName);
                    if (null != eventWrapper)
                    {
                        // Create the event payload
                        if (! string.IsNullOrWhiteSpace(eventName) )
                        {
                            // Get the event type from the map
                            if (null != _eventMaps )
                            {
                                eventWrapper = _eventMaps.CreateEventClass(eventName);  
                            }
                        }
                        if (null != eventWrapper.EventPayload)
                        {
                            // Populate the properties from the table row
                            foreach (string entityProperty in dteRow.Keys )
                            {
                                if (!IsContextProperty(entityProperty))
                                {
                                    PropertyInfo pi = eventWrapper.EventPayload.GetType().GetProperty(entityProperty);
                                    if (null != pi)
                                    {
                                        if (pi.CanWrite)
                                        {
                                            object fieldValue;

                                            if (dteRow.TryGetValue(entityProperty, out fieldValue))
                                            {
                                                if (!IsPropertyValueEmpty(pi,
                                                    fieldValue ))
                                                {
                                                    if (pi.PropertyType.IsEnum)
                                                    {
                                                        // Emums are stored as strings
                                                        string propertyValue = dteRow.GetString(entityProperty);
                                                        object value;
                                                        if (Enum.TryParse(pi.PropertyType,
                                                            propertyValue, out value))
                                                        {
                                                            pi.SetValue(eventWrapper.EventPayload, value);
                                                        }
                                                    }
                                                    else
                                                    {
                                                        // If it is a decimal it may be saved as a double ...
                                                        if (pi.PropertyType == typeof(decimal))
                                                        {
                                                            if (fieldValue.GetType() == typeof(double))
                                                            {
                                                                fieldValue = Convert.ToDecimal( fieldValue);
                                                            }
                                                        }

                                                        // If it is a datetime it may be saved as a datetimeoffset ...
                                                        if (pi.PropertyType == typeof(DateTime))
                                                        {
                                                            if (fieldValue.GetType() == typeof(DateTimeOffset ))
                                                            {
                                                                fieldValue = ((DateTimeOffset)fieldValue).UtcDateTime ;
                                                            }
                                                        }

                                                        pi.SetValue(eventWrapper.EventPayload,
                                                            fieldValue);
                                                    }
                                                }
                                            }
                                        }
                                        }
                                    }
                                }
                        }

                        return eventWrapper;
                    }
                }
            }

            // Not enough data to make a wrapped event
            return null;
        }

        /// <summary>
        /// Create the event class to hold the event properties from the event name
        /// </summary>
        /// <param name="eventName">
        /// The event name from the wrapped event record
        /// </param>
        private IEvent CreateEventClass(string eventName)
        {
            return _eventMaps.CreateEventClass( eventName);
        }

        /// <summary>
        /// Create a query to get the events greater than or equal to the given starting point
        /// </summary>
        /// <param name="StartingSequenceNumber">
        /// The 1-based sequence number to start from
        /// </param>
        private string CreateQuery(int StartingSequenceNumber)
        {
            return TableClient.CreateQueryFilter($"PartitionKey eq {InstanceKey} and RowKey ge {SequenceNumberAsString(StartingSequenceNumber)} ");
        }

        /// <summary>
        /// Get all the "key" rows in the table
        /// </summary>
        private string GetKeysQuery()
        {
            return TableClient.CreateQueryFilter($"RowKey eq {SequenceNumberAsString(0)} ");
        }




        public async Task<IEnumerable<string>> GetAllInstanceKeys(DateTime? asOfDate)
        {

            if (Table != null)
            {
                List<string> ret = new List<string>();

                Azure.Pageable<TableEntity> getInstancesQuery = Table.Query<TableEntity>(filter: GetKeysQuery());

                foreach (var dteRow in getInstancesQuery)
                {
                    bool include = true;
                    if (dteRow.ContainsKey(nameof(TableEntityKeyRecord.Deleting)))
                    {
                        if (dteRow.GetBoolean(nameof(TableEntityKeyRecord.Deleting)).GetValueOrDefault(false))
                        {
                            include = false;
                        }
                    }
                    if (include)
                    {
                        // add the "key"
                        if (dteRow.ContainsKey(nameof(InstanceKey)))
                        {
                            ret.Add(dteRow.GetString(nameof(InstanceKey)));
                        }
                    }
                }
                
                return ret;
            }


            return Enumerable.Empty<string>();
        }

        public TableEventStreamReader(IEventStreamIdentity identity,
            string connectionStringName = @"",
            IEventMaps eventMaps = null)
            : base(identity, false, connectionStringName )
        {


            // if event maps not passed in you have to create a default
            if (null == eventMaps)
            {
                _eventMaps = EventMaps.CreateDefaultEventMaps();
            }
            else
            {
                _eventMaps = eventMaps;
            }
        }


        /// <summary>
        /// Creates an azure table storage based event stream reader for the given aggregate
        /// </summary>
        /// <param name="identity">
        /// The unique identifier of the event stream to read
        /// </param>
        /// <param name="connectionStringName">
        /// The name of the connection string to use to do the reading from the table
        /// </param>
        public static TableEventStreamReader Create(IEventStreamIdentity identity,
            string connectionStringName = @"")
        {
            return new TableEventStreamReader(identity, connectionStringName);
        }
        
        public static ProjectionProcessor CreateProjectionProcessor(IEventStreamIdentity identity,
            string connectionStringName = @"")
        {
            return new ProjectionProcessor(Create(identity, connectionStringName));
        }

        public static ClassificationProcessor CreateClassificationProcessor(IEventStreamIdentity identity,
              string connectionStringName = @"")
        {
            return new ClassificationProcessor(Create(identity, connectionStringName));
        }


    }
}
