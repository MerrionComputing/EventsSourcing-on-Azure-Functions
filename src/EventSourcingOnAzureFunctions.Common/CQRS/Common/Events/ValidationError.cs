using EventSourcingOnAzureFunctions.Common.EventSourcing;
using System;


namespace EventSourcingOnAzureFunctions.Common.CQRS.Common.Events
{
    /// <summary>
    /// A validation error occured with the parameters for a command or query
    /// </summary>
    [EventName("Validation Error")]
    public class ValidationError
    {

        /// <summary>
        /// The name of the parameter that had a validation error
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Does the validation error prevent the command or query from proceeding
        /// </summary>
        public bool Fatal { get; set; }

        /// <summary>
        /// The message to go along with the validation notification
        /// </summary>
        public string Message { get; set; }

    }
}
