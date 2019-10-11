using System;
using System.Collections.Generic;
using System.Text;

namespace EventSourcingOnAzureFunctions.Common.EventSourcing.Interfaces
{
    /// <summary>
    /// Additional context information provided when an event is written
    /// </summary>
    /// <remarks>
    /// This is non-business information for logging, auditing etc.
    /// </remarks>
    public interface IWriteContext
    {

        /// <summary>
        /// Which user caused the event to be written
        /// </summary>
        /// <remarks>
        /// This can be empty in the case of timer or state triggered events
        /// </remarks>
        string Who { get; }

        /// <summary>
        /// The source from whence this event originated
        /// </summary>
        /// <remarks>
        /// This could be an IP address or a process name or whatever makes sense
        /// </remarks>
        string Source { get; }

        /// <summary>
        /// Any additional non-business comments attached to the event 
        /// </summary>
        /// <remarks>
        /// This could be used for audit purposes for example
        /// </remarks>
        string Commentary { get; }

        /// <summary>
        /// An externally provided unique identifier to tie together events that are linked together
        /// </summary>
        string CorrelationIdentifier { get; }

        /// <summary>
        /// An externally provided unique identifier to tie together events comming from the same cause
        /// </summary>
        string CausationIdentifier { get;}
    }
}
