using System;
using System.Collections.Generic;
using System.Text;

namespace EventSourcingOnAzureFunctions.Common.EventSourcing.Interfaces
{
    /// <summary>
    /// The result of an append event operation
    /// </summary>
    public interface IAppendResult
    {

        /// <summary>
        /// Did this event append result in a new event stream being created
        /// </summary>
        /// <remarks>
        /// This is needed in order to raise the "New Entity" notification
        /// </remarks>
        bool NewEventStreamCreated { get; }

        /// <summary>
        /// The sequence number of the event that was just appended
        /// </summary>
        int SequenceNumber { get; }
    }
}
