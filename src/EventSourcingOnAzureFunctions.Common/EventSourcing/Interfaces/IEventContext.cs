using System;
using System.Collections.Generic;
using System.Text;

namespace EventSourcingOnAzureFunctions.Common.EventSourcing.Interfaces
{
    /// <summary>
    /// Event and its context information provided when it was written
    /// </summary>
    public interface IEventContext
        : IWriteContext
    {

        /// <summary>
        /// The version number of the event schema 
        /// </summary>
        int VersionNumber { get; }

        /// <summary>
        /// The incremental sequence number of this event in the stream/history in which it is written
        /// </summary>
        int SequenceNumber { get; }

        /// <summary>
        /// The specific event in this context
        /// </summary>
        IEvent EventInstance {get;}

    }
}
