using EventSourcingOnAzureFunctions.Common.EventSourcing;
using EventSourcingOnAzureFunctions.Common.EventSourcing.Interfaces;
using System;

namespace EventSourcingOnAzureFunctions.Common.CQRS.CommandHandler.Events
{

    /// <summary>
    /// A named step that is part of executing this command has completed
    /// </summary>
    [EventName("Command Step Completed")]
    public class StepCompleted
        : IEventStreamIdentity
    {

        /// <summary>
        /// The name of the step that was just completed
        /// </summary>
        public string StepName { get; set; }

        /// <summary>
        /// The message returned in completing the step
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// The date/time the command step completion was logged by the system
        /// </summary>
        public DateTime DateLogged { get; set; }


        /// <summary>
        /// The domain of the entity to which the command step was applied
        /// </summary>
        public string DomainName { get; set; }

        /// <summary>
        /// The entity type of the entity to which the command step was applied
        /// </summary>
        public string EntityTypeName { get; set; }

        /// <summary>
        /// The instance unique key of the entity to which the command step was applied
        /// </summary>
        public string InstanceKey { get; set; }
    }
}
