using EventSourcingOnAzureFunctions.Common.EventSourcing.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace EventSourcingOnAzureFunctions.Common.EventSourcing.Implementation
{
    /// <summary>
    /// A write context to wrap around an event in order to give additional non-business context for the event 
    /// </summary>
    public class WriteContext
        : IWriteContext
    {
        public string Who { get; }

        public string Source { get; }

        public string Commentary { get; }

        public string CorrelationIdentifier { get; }


        public static IWriteContext DefaultWriterContext()
        {
            // todo - set default values... maybe from config or..?
            return new WriteContext(); 
        }
    }
}
