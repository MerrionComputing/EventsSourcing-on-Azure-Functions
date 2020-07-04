using EventSourcingOnAzureFunctions.Common.EventSourcing;
using EventSourcingOnAzureFunctions.Common.EventSourcing.Interfaces;
using System;
using System.Collections.Generic;

namespace EventSourcingOnAzureFunctions.Common.ClassifierHandler.Events
{

    [EventName("Classification Requested")]
    public class ClassifierRequested
        : IClassifierRequest
    {

        /// <summary>
        /// The domain name of the event stream over which the classification is 
        /// to be run
        /// </summary>
        public string DomainName { get; set; }

        /// <summary>
        /// The entity type for which the classification will be run
        /// </summary>
        public string EntityTypeName { get; set; }

        /// <summary>
        /// The unique instance of the event stream over which the 
        /// classification should run
        /// </summary>
        public string InstanceKey { get; set; }

        /// <summary>
        /// The name of the classification to run over that event stream
        /// </summary>
        public string ClassifierTypeName { get; set; }

        /// <summary>
        /// The date up-to which we want the classification to be run
        /// </summary>
        public Nullable<DateTime> AsOfDate { get; set; }

        /// <summary>
        /// The date/time the classification request was logged by the system
        /// </summary>
        public DateTime DateLogged { get; set; }

        /// <summary>
        /// An unique identifier set by the caller to trace this classifier operation
        /// </summary> 
        public string CorrelationIdentifier { get; set; }


        /// <summary>
        /// Any parameters included in the classifier request
        /// </summary>
        Dictionary<string, object> Parameters { get; set; }
    }
}
