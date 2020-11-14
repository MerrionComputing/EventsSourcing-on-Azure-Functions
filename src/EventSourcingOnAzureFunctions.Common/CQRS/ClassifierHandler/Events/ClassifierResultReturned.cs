using EventSourcingOnAzureFunctions.Common.EventSourcing;
using EventSourcingOnAzureFunctions.Common.EventSourcing.Interfaces;
using System;
using static EventSourcingOnAzureFunctions.Common.EventSourcing.ClassificationResponse;

namespace EventSourcingOnAzureFunctions.Common.CQRS.ClassifierHandler.Events
{
    [EventName("Classification Returned")]
    public class ClassifierResultReturned
        : IClassifierRequest
    {


        /// <summary>
        /// The domain name of the event stream over which the classifier was run
        /// </summary>
        public string DomainName { get; set; }

        /// <summary>
        /// The entity type for which the classifier was run
        /// </summary>
        public string EntityTypeName { get; set; }

        /// <summary>
        /// The unique instance of the event stream over which the 
        /// classifier was run
        /// </summary>
        public string InstanceKey { get; set; }

        /// <summary>
        /// The name of the classifier we ran over that event stream
        /// </summary>
        public string ClassifierTypeName { get; set; }

        /// <summary>
        /// The date up-to which the classifier was run
        /// </summary>
        public Nullable<DateTime> AsOfDate { get; set; }


        /// <summary>
        /// The sequence number of the last event read when running the classifier
        /// </summary>
        /// <remarks>
        /// This can be used for concurrency protection
        /// </remarks>
        public int AsOfSequenceNumber { get; set; }

        /// <summary>
        /// The result of the classification - is this instance a member of whatever
        /// group clause the classification represents
        /// </summary>
        public ClassificationResults Result { get; set; }


        /// <summary>
        /// An unique identifier set by the caller to trace this classifier operation
        /// </summary> 
        public string CorrelationIdentifier { get; set; }
    }
}
