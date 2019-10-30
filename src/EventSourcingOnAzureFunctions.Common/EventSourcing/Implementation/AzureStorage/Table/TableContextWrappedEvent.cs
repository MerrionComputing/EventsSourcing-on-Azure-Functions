using EventSourcingOnAzureFunctions.Common.EventSourcing.Interfaces;
using Microsoft.Azure.Cosmos.Table;
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
        public int VersionNumber { get; private set; }

        public int SequenceNumber { get; private set; }

        public IEvent EventInstance { get; private set; }

        public string Who { get; private set; }

        public string Source { get; private set; }

        public string Commentary { get; private set; }

        public string CorrelationIdentifier { get; private set; }

        public string CausationIdentifier { get; private set; }

        /// <summary>
        /// The name or URN of the schema used to write this event
        /// </summary>
        public string SchemaName { get; set; }

        public DateTimeOffset  EventWrittenDateTime { get; set; }

        public TableContextWrappedEvent(IEvent eventToWrap, 
            DynamicTableEntity dteRow)
        {

            EventInstance = eventToWrap;

            if (null != dteRow )
            {
                if (dteRow.Properties.ContainsKey(TableEventStreamBase.FIELDNAME_COMMENTS) )
                {
                    Commentary = dteRow.Properties[TableEventStreamBase.FIELDNAME_COMMENTS].StringValue;
                }
                if (dteRow.Properties.ContainsKey(TableEventStreamBase.FIELDNAME_CORRELATION_IDENTIFIER ) )
                {
                    CorrelationIdentifier = dteRow.Properties[TableEventStreamBase.FIELDNAME_CORRELATION_IDENTIFIER].StringValue;
                }
                if (dteRow.Properties.ContainsKey(TableEventStreamBase.FIELDNAME_CAUSATION_IDENTIFIER))
                {
                    CausationIdentifier = dteRow.Properties[TableEventStreamBase.FIELDNAME_CAUSATION_IDENTIFIER].StringValue;
                }
                if (dteRow.Properties.ContainsKey(TableEventStreamBase.FIELDNAME_SOURCE )  )
                {
                    Source = dteRow.Properties[TableEventStreamBase.FIELDNAME_SOURCE].StringValue;
                }
                if (dteRow.Properties.ContainsKey(TableEventStreamBase.FIELDNAME_VERSION ) )
                {
                    VersionNumber = dteRow.Properties[TableEventStreamBase.FIELDNAME_VERSION].Int32Value.GetValueOrDefault(1); 
                }
                if (dteRow.Properties.ContainsKey(TableEventStreamBase.FIELDNAME_WHO  ))
                {
                    Who = dteRow.Properties[TableEventStreamBase.FIELDNAME_WHO].StringValue; 
                }
                string sequenceNumberAsString = dteRow.RowKey;
                if (! string.IsNullOrWhiteSpace(sequenceNumberAsString ) )
                {
                    SequenceNumber = TableEventStreamBase.SequenceNumberFromString(sequenceNumberAsString);
                }

                EventWrittenDateTime = dteRow.Timestamp;
            }
        }
    }
}
