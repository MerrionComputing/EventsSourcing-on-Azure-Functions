using EventSourcingOnAzureFunctions.Common.EventSourcing.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace EventSourcingOnAzureFunctions.Common.EventSourcing.Implementation
{
    public class AppendResult
        : IAppendResult
    {

        /// <summary>
        /// Did this write operation result in the creation of a new event stream
        /// </summary>
        public bool NewEventStreamCreated { get; private  set; }

        /// <summary>
        /// The sequence number of the event written
        /// </summary>
        public int SequenceNumber { get; private set; }


        public AppendResult(bool newEntity,
            int sequenceNumber)
        {
            NewEventStreamCreated = newEntity;
            SequenceNumber = sequenceNumber;
        }

    }
}
