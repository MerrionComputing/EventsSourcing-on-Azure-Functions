using EventSourcingOnAzureFunctions.Common.EventSourcing;
using System;
using System.Collections.Generic;
using System.Text;

namespace EventSourcingOnAzureFunctions.Common.CQRS.Common.Events
{
    [EventName("Validation Completed")]
    public class ValidationCompleted
    {

        /// <summary>
        /// Were any errors encountered while validating this command or 
        /// query
        /// </summary>
        public bool ErrorsEnountered { get; set; }


        /// <summary>
        /// The validation message to log for the completion of the validation step
        /// </summary>
        public string ValidationMessage { get; set; }
    }
}
