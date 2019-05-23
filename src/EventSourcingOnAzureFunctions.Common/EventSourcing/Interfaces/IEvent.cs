using System;
using System.Collections.Generic;
using System.Text;

namespace EventSourcingOnAzureFunctions.Common.EventSourcing.Interfaces
{
    /// <summary>
    /// The additional data required for every event 
    /// </summary>
    public interface IEvent
    {

        /// <summary>
        /// The version number of the event schema 
        /// </summary>
        int VersionNumber { get; }

        /// <summary>
        /// The incremental sequence number of this event in the stream/history in which it is written
        /// </summary>
        int SequenceNumber { get; }


    }
}
