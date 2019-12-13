using EventSourcingOnAzureFunctions.Common.EventSourcing;
using System;

namespace EventSourcingOnAzureFunctions.Common.CQRS.CommandHandler.Events
{
    /// <summary>
    /// A multi-step (event stream backed) command has completed
    /// </summary>
    /// <remarks>
    /// Once a query is tagged as completed it cannot be reopened so this 
    /// should be the last event in the event stream
    /// </remarks>
    [EventName("Command Created")]
    public class Completed
    {
        /// <summary>
        /// The date/time the command was completed by the system
        /// </summary>
        public DateTime DateCompleted { get; set; }

        /// <summary>
        /// Commentary on the command that has completed
        /// </summary>
        public string Notes { get; set; }
    }
}
