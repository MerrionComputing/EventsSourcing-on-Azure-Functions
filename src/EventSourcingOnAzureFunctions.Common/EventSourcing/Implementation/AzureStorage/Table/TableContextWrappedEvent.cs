using EventSourcingOnAzureFunctions.Common.EventSourcing.Interfaces;
using Azure.Data.Tables;
using System;
using System.Collections.Generic;
using System.Text;

namespace EventSourcingOnAzureFunctions.Common.EventSourcing.Implementation.AzureStorage.Table
{
    /// <summary>
    /// A class to wrap an event in the context read from an Azure table
    /// </summary>
    public class TableContextWrappedEvent
        : IEventContext
    {
        /// <summary>
        /// The version number of the event schema 
        /// </summary>
        public int VersionNumber { get; private set; }

        /// <summary>
        /// The sequence number of this event in its event stream
        /// </summary>
        public int SequenceNumber { get; private set; }

        /// <summary>
        /// The specific event data
        /// </summary>
        public IEvent EventInstance { get; private set; }

        /// <summary>
        /// The user that was the source of the event
        /// </summary>
        public string Who { get; private set; }

        /// <summary>
        /// The application that was the source of the event
        /// </summary>
        public string Source { get; private set; }

        /// <summary>
        /// Additional notes to go along with the event instance
        /// </summary>
        public string Commentary { get; private set; }

        /// <summary>
        /// A unique identifier to show events that are correlated together
        /// </summary>
        public string CorrelationIdentifier { get; private set; }

        /// <summary>
        /// The unique identifier of whatever caused this event to occur
        /// </summary>
        public string CausationIdentifier { get; private set; }

        /// <summary>
        /// The name or URN of the schema used to write this event
        /// </summary>
        public string SchemaName { get; set; }

        /// <summary>
        /// The date and time the event was written
        /// </summary>
        public DateTimeOffset  EventWrittenDateTime { get; set; }

        public TableContextWrappedEvent(IEvent eventToWrap, 
            TableEntity dteRow)
        {

            EventInstance = eventToWrap;

            if (null != dteRow )
            {
                if (dteRow.ContainsKey(TableEventStreamBase.FIELDNAME_COMMENTS) )
                {
                    Commentary = dteRow.GetString(TableEventStreamBase.FIELDNAME_COMMENTS);
                }
                if (dteRow.ContainsKey(TableEventStreamBase.FIELDNAME_CORRELATION_IDENTIFIER ) )
                {
                    CorrelationIdentifier = dteRow.GetString(TableEventStreamBase.FIELDNAME_CORRELATION_IDENTIFIER);
                }
                if (dteRow.ContainsKey(TableEventStreamBase.FIELDNAME_CAUSATION_IDENTIFIER))
                {
                    CausationIdentifier = dteRow.GetString(TableEventStreamBase.FIELDNAME_CAUSATION_IDENTIFIER);
                }
                if (dteRow.ContainsKey(TableEventStreamBase.FIELDNAME_SOURCE )  )
                {
                    Source = dteRow.GetString(TableEventStreamBase.FIELDNAME_SOURCE);
                }
                if (dteRow.ContainsKey(TableEventStreamBase.FIELDNAME_VERSION ) )
                {
                    VersionNumber = dteRow.GetInt32(TableEventStreamBase.FIELDNAME_VERSION).GetValueOrDefault(1); 
                }
                if (dteRow.ContainsKey(TableEventStreamBase.FIELDNAME_WHO))
                {
                    Who = dteRow.GetString(TableEventStreamBase.FIELDNAME_WHO); 
                }
                string sequenceNumberAsString = dteRow.RowKey;
                if (! string.IsNullOrWhiteSpace(sequenceNumberAsString ) )
                {
                    SequenceNumber = TableEventStreamBase.SequenceNumberFromString(sequenceNumberAsString);
                }

                EventWrittenDateTime = dteRow.Timestamp.GetValueOrDefault();
            }
        }
    }
}
