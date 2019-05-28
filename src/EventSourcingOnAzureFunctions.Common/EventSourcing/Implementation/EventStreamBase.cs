using System;
using System.Collections.Generic;
using System.Text;

namespace EventSourcingOnAzureFunctions.Common.EventSourcing.Implementation
{
    /// <summary>
    /// Base functionality for all event streams, regardless of their backing storage mechanism
    /// </summary>
    public abstract class EventStreamBase
    {


        /// <summary>
        /// Constraints to control writing to the underlying event stream
        /// </summary>
        public enum EventStreamExistenceConstraint
        {
            /// <summary>
            /// It doesn't matter if the event stream already exists or not
            /// </summary>
            Loose = 0,
            /// <summary>
            /// This command may only proceed if the event stream already exists
            /// </summary>
            MustExist = 1,
            /// <summary>
            /// This command may only succeed if the event stream does not already exist
            /// </summary>
            MustBeNew = 2
        }

    }
}
