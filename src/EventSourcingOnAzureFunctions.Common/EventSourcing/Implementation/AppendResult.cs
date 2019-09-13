using EventSourcingOnAzureFunctions.Common.EventSourcing.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace EventSourcingOnAzureFunctions.Common.EventSourcing.Implementation
{
    public class AppendResult
        : IAppendResult
    {
        public bool NewEventStreamCreated { get; private  set; }

        public int SequenceNumber { get; private set; }


        public AppendResult(bool newEntity,
            int sequenceNumber)
        {
            NewEventStreamCreated = NewEventStreamCreated;
            SequenceNumber = sequenceNumber;
        }

    }
}
