using EventSourcingOnAzureFunctions.Common.EventSourcing;
using EventSourcingOnAzureFunctions.Common.EventSourcing.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace EventSourcingOnAzureFunctions.Common.CQRS.CommandHandler.Events
{
    /// <summary>
    /// A processing step was initiated for a given command
    /// </summary>
    [EventName("Command Step Initiated")]
    public class CommandStepInitiated
        : IEventStreamIdentity
    {

        /// <summary>
        /// The name of the step that was initiated
        /// </summary>
        public string StepName { get; set; }

        /// <summary>
        /// The domain of the entity to which the command step is being applied
        /// </summary>
        public string DomainName { get; set; }

        /// <summary>
        /// The entity type of the entity to which the command step is being applied
        /// </summary>
        public string EntityTypeName { get; set; }

        /// <summary>
        /// The instance unique key of the entity to which the command step is being applied
        /// </summary>
        public string InstanceKey { get; set; }

    }
}
