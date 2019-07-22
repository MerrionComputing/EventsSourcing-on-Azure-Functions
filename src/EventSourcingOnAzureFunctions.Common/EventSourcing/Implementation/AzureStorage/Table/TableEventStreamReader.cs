using EventSourcingOnAzureFunctions.Common.EventSourcing.Interfaces;
using Microsoft.Azure.Cosmos.Table;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace EventSourcingOnAzureFunctions.Common.EventSourcing.Implementation.AzureStorage.Table
{
    public sealed class TableEventStreamReader
        : TableEventStreamBase, IEventStreamReader
    {


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

            throw new NotImplementedException();
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

                TableContinuationToken token = new TableContinuationToken();

                TableQuery getEventsQuery = CreateQuery(StartingSequenceNumber);

                do
                {
                    // create the query to be executed..
                    var segment = await Table.ExecuteQuerySegmentedAsync(getEventsQuery,
                         token,
                         requestOptions: GetDefaultRequestOptions(),
                         operationContext: GetDefaultOperationContext());

                    foreach (DynamicTableEntity  dteRow in segment )
                    {
                        // Process this one row as an event...
                        // Get the event data
                        IEvent eventPayload = GetEventFromDynamicTableEntity(dteRow);
                        if (null != eventPayload)
                        {
                            // Wrap it in the event context
                            IEventContext wrappedEvent = WrapEventFromDynamicTableEntity(dteRow, eventPayload);
                            if (null != wrappedEvent)
                            {
                                // Add it to the end of the list
                                ret.Add(wrappedEvent);
                            }
                        }
                    }

                    // update the continuation token to get the next chunk of records
                    token = segment.ContinuationToken;

                } while (null != token );


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

        private IEventContext WrapEventFromDynamicTableEntity(DynamicTableEntity dteRow, 
            IEvent eventPayload)
        {
            if (null != eventPayload )
            {
                return new TableContextWrappedEvent(eventPayload, dteRow);
            }

            // Not enough data to make a wrapped event
            return null;
        }


        private IEvent GetEventFromDynamicTableEntity(DynamicTableEntity dteRow)
        {
            if (null != dteRow )
            {
                // get the event name
                string eventName = null;

                if (dteRow.Properties.ContainsKey(FIELDNAME_EVENTTYPE))
                {
                    eventName = dteRow.Properties[FIELDNAME_EVENTTYPE].StringValue;
                }

                if (!string.IsNullOrWhiteSpace(eventName))
                {
                    IEvent eventPayload = CreateEventClass(eventName);
                    if (null != eventPayload )
                    {
                        // Populate the properties from the table row
                        foreach (var  entityProperty in dteRow.Properties )
                        {
                            if (! IsContextProperty(entityProperty.Key ))
                            {
                                PropertyInfo pi = eventPayload.GetType().GetProperty(entityProperty.Key); 
                                if (null != pi)
                                {
                                    if (pi.CanWrite )
                                    {
                                        pi.SetValue(eventPayload, entityProperty.Value);
                                    }
                                }
                            }
                        }

                        return eventPayload;
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
            throw new NotImplementedException();
        }

        /// <summary>
        /// Create a query to get the events greater than or equal to the given starting point
        /// </summary>
        /// <param name="StartingSequenceNumber">
        /// The 1-based sequence number to start from
        /// </param>
        private TableQuery CreateQuery(int StartingSequenceNumber)
        {
            return new TableQuery()
                .Where(TableQuery.CombineFilters(
                    TableQuery.GenerateFilterCondition("PartitionKey", 
                          QueryComparisons.Equal, 
                          InstanceKey),
                    TableOperators.And,
                    TableQuery.GenerateFilterCondition("RowKey",
                         QueryComparisons.GreaterThanOrEqual,
                         SequenceNumberAsString(StartingSequenceNumber))
                    )
                );
        }

        /// <summary>
        /// Get the standard request options to use when retrieving the event stream data from a table
        /// </summary>
        private TableRequestOptions GetDefaultRequestOptions()
        {
            return new TableRequestOptions()
            {
                 PayloadFormat = TablePayloadFormat.Json,
                  TableQueryMaxItemCount = MAX_BATCH_SIZE
            };
        }


        /// <summary>
        /// Policy to allow read-access to the tables
        /// </summary>
        public SharedAccessAccountPolicy DefaultSharedAccessAccountPolicy
        {
            get
            {
                // Make a standard shared access policy to use 
                return new SharedAccessAccountPolicy()
                {
                    Permissions =  SharedAccessAccountPermissions.Read
                    | SharedAccessAccountPermissions.List
                    ,
                    ResourceTypes = SharedAccessAccountResourceTypes.Object,
                    Services = SharedAccessAccountServices.Table,
                    Protocols = SharedAccessProtocol.HttpsOnly
                };
            }
        }

        public SharedAccessTablePolicy DefaultSharedAccessTablePolicy
        {
            get
            {
                return new SharedAccessTablePolicy()
                {
                    Permissions = SharedAccessTablePermissions.Query 
                };
            }
        }

        public TableEventStreamReader(IEventStreamIdentity identity,
            string connectionStringName = @"")
            : base(identity, false, connectionStringName )
        {

        }
    }
}
