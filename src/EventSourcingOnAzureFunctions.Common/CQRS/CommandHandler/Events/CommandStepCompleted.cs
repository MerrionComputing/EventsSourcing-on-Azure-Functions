using EventSourcingOnAzureFunctions.Common.EventSourcing;
using System;

namespace EventSourcingOnAzureFunctions.Common.CQRS.CommandHandler.Events
{

    /// <summary>
    /// A named step that is part of executing this command has completed
    /// </summary>
    [EventName("Command Step Completed")]
    public class StepCompleted
    {

        /// <summary>
        /// The name of the step that was just completed
        /// </summary>
        public string StepName { get; set; }


        /// <summary>
        /// The date/time the command step completion was logged by the system
        /// </summary>
        public DateTime DateLogged { get; set; }


    }
}
