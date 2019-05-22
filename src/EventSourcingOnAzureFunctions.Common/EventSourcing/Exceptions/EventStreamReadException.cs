using EventSourcingOnAzureFunctions.Common.EventSourcing.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace EventSourcingOnAzureFunctions.Common.EventSourcing.Exceptions
{
    /// <summary>
    /// An exception that has occured while reading 
    /// </summary>
    public class EventStreamReadException
        : EventStreamExceptionBase
    {



        public EventStreamReadException(IEventStreamIdentity eventStreamIdentity,
                int sequenceNumber,
                string message = "",
                Exception innerException = null,
                string source = "")
            : base(eventStreamIdentity,
                  sequenceNumber,
                  message,
                  innerException,
                  source )
        {

        }
    }
}
