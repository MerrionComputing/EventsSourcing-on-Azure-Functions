using EventSourcingOnAzureFunctions.Common.EventSourcing;
using EventSourcingOnAzureFunctions.Common.EventSourcing.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;

namespace EventSourcingOnAzureFunctions.Common.CQRS.ClassifierHandler.Events
{

    [EventName("Classification Requested")]
    public sealed class ClassifierRequested
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

        /// <summary>
        /// Parameter-less constructor for serialisation
        /// </summary>
        public ClassifierRequested()
        {
        }


        /// <summary>
        /// Turn a queue notification message back into a projection requested event
        /// </summary>
        /// <param name="queuedMessage">
        /// The message sent by the <see cref="Notification.QueueNotificationDispatcher"/> for the command or query
        /// </param>
        /// <returns>
        /// If the string can be turned into a projection request then it is - otherwise null
        /// </returns>
        /// <remarks>
        /// Message is pipe separated
        /// E|Projection Requested|{command/query id}|{command/query sequence number}|null||{ProjectionDomainName}|{ProjectionEntityTypeName}|{ProjectionInstanceKey}|{ProjectionTypeName}|{AsOfDate}|{CorrelationIdentifier}
        /// </remarks>
        public static ClassifierRequested FromQueuedMessage(string queuedMessage)
        {
            if (!string.IsNullOrWhiteSpace(queuedMessage))
            {
                string[] messageParts = queuedMessage.Split('|');
                if (messageParts.Count() > 5)
                {
                    // Message has parts that can be converted to a projection request
                    ClassifierRequested request = new ClassifierRequested();
                    if (messageParts.Count() >= 6)
                    {
                        request.DomainName = messageParts[5];
                    }
                    if (messageParts.Count() >= 7)
                    {
                        request.EntityTypeName = messageParts[6];
                    }
                    if (messageParts.Count() >= 8)
                    {
                        request.InstanceKey = messageParts[7];
                    }
                    if (messageParts.Count() >= 9)
                    {
                        request.ClassifierTypeName  = messageParts[8];
                    }
                    if (messageParts.Count() >= 10)
                    {
                        //AsOfDate
                        if (!string.IsNullOrWhiteSpace(messageParts[9]))
                        {
                            DateTime asOf;
                            if (DateTime.TryParse(messageParts[9], out asOf))
                            {
                                request.AsOfDate = asOf;
                            }
                        }
                    }
                    if (messageParts.Count() >= 11)
                    {
                        request.CorrelationIdentifier = messageParts[10];
                    }

                    return request;
                }
            }
            return null;
        }

        /// <summary>
        /// Turn a classification requested event to the <see cref="Notification.QueueNotificationDispatcher"/> type of message
        /// </summary>
        /// <param name="evtPayload">
        /// The event for a classification being requested
        /// </param>
        public static string ToQueueMessage(ClassifierRequested evtPayload)
        {
            if (evtPayload != null)
            {
                string asOf = "null";
                if (evtPayload.AsOfDate.HasValue)
                {
                    asOf = evtPayload.AsOfDate.Value.ToString("O");
                }
                return $"|{evtPayload.DomainName}|{evtPayload.EntityTypeName}|{evtPayload.InstanceKey}|{evtPayload.ClassifierTypeName}|{asOf}|{evtPayload.CorrelationIdentifier}";
            }
            return "";
        }
    }
}
