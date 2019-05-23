using EventSourcingOnAzureFunctions.Common.EventSourcing.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace EventSourcingOnAzureFunctions.Common.EventSourcing.Exceptions
{
    public class EventStreamWriteException
: EventStreamExceptionBase
    {



        public EventStreamWriteException(IEventStreamIdentity eventStreamIdentity,
                int sequenceNumber,
                string message = "",
                Exception innerException = null,
                string source = "")
            : base(eventStreamIdentity,
                  sequenceNumber,
                  message,
                  innerException,
                  source)
        {

        }
    }
}
