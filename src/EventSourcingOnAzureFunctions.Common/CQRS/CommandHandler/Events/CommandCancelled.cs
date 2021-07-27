using EventSourcingOnAzureFunctions.Common.EventSourcing;
using System;
using System.Collections.Generic;
using System.Text;

namespace EventSourcingOnAzureFunctions.Common.CQRS.CommandHandler.Events
{

    /// <summary>
    /// A multi-step (event stream backed) command has been cancelled
    /// </summary>
    [EventName("Command Cancelled")]
    public sealed class CommandCancelled
    {

        /// <summary>
        /// Did the cancellation initiate a rollback via compensating events
        /// </summary>
        public bool CompensationInitiated { get; set; }

        /// <summary>
        /// The date/time the command was cancelled
        /// </summary>
        public DateTime DateCancelled { get; set; }

        /// <summary>
        /// Commentary on why the command that has been cancelled
        /// </summary>
        public string Notes { get; set; }

    }
}
