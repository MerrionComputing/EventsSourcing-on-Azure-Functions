using EventSourcingOnAzureFunctions.Common.ClassifierHandler.Events;
using EventSourcingOnAzureFunctions.Common.EventSourcing;
using EventSourcingOnAzureFunctions.Common.EventSourcing.Interfaces;
using System;

namespace EventSourcingOnAzureFunctions.Common.CQRS.ClassifierHandler.Events
{
    [EventName("Classification Parameter Set")]
    public class ClassifierRequestParameterSet
    {

        /// <summary>
        /// An unique identifier set by the caller to trace this classifier operation
        /// </summary> 
        public string CorrelationIdentifier { get; set; }

        /// <summary>
        /// The name of the classification parameter
        /// </summary>
        public string ParameterName { get; set; }

        /// <summary>
        /// The value assigned to the parameter
        /// </summary>
        public object ParameterValue { get; set; }
    }
}
