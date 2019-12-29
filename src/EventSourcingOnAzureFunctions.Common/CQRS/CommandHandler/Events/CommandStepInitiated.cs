using EventSourcingOnAzureFunctions.Common.EventSourcing;
using System;
using System.Collections.Generic;
using System.Text;

namespace EventSourcingOnAzureFunctions.Common.CQRS.CommandHandler.Events
{
    [EventName("Command Step Initiated")]
    public class CommandStepInitiated
    {

        /// <summary>
        /// The name of the step that was initiated
        /// </summary>
        public string StepName { get; set; }

    }
}
