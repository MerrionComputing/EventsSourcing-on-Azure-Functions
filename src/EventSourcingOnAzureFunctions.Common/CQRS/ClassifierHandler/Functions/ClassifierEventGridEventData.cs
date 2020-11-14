using EventSourcingOnAzureFunctions.Common.CQRS.ClassifierHandler.Events;
using EventSourcingOnAzureFunctions.Common.EventSourcing.Implementation;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace EventSourcingOnAzureFunctions.Common.CQRS.ClassifierHandler.Functions
{
    /// <summary>
    /// The data sent in an OnClassificationRequested event
    /// </summary>
    /// <remarks>
    ///     "data": { 
    ///          "notificationId": "6eb417e5d301402a96d212db3601caa5", 
    ///          "domainName": "Bank", 
    ///          "entityTypeName": "Query", --or "Command", 
    ///          "instanceKey": "a2a92eda-0ebc-4ca6-8f0e-27a867f6a2fe", 
    ///          "commentary": null, 
    ///          "eventType": "Classification Requested", 
    ///          "sequenceNumber": 1320, 
    ///          "eventPayload": {  --from ClassifierRequested event
    ///              "domainName": "Bank", 
    ///              etc..
    ///              }, 
    ///         "context": { 
    ///             "Who": null, 
    ///             "Source": "WithdrawMoney", 
    ///             "Commentary": null, 
    ///             "CorrelationIdentifier": null, 
    ///             "CausationIdentifier": "e9d72eda-0ebc-4c66-8fce-27a867f6f2fe", 
    ///             "SchemaName": null } 
    ///      }
    /// </remarks>
    public sealed class ClassifierRequestedEventGridEventData
    {

        /// <summary>
        /// The notification instance identifier (for logical idempotency checking)
        /// </summary>
        [JsonProperty(PropertyName = "notificationId")]
        public string NotificationId { get; set; }

        /// <summary>
        /// The domain in which the classifier is to be run
        /// </summary>
        [JsonProperty(PropertyName = "domainName")]
        public string DomainName { get; set; }

        /// <summary>
        /// The type of entity for which the classifier is to be run 
        /// </summary>
        /// <remarks>
        /// Can be "Query" or "Command" 
        /// </remarks>
        [JsonProperty(PropertyName = "entityTypeName")]
        public string EntityTypeName { get; set; }

        /// <summary>
        /// The unique identifier of the command or query for which the classifier 
        /// is to be run
        /// </summary>
        [JsonProperty(PropertyName = "instanceKey")]
        public string InstanceKey { get; set; }


        /// <summary>
        /// Additional commentary about the classifier request
        /// </summary>
        [JsonProperty(PropertyName = "commentary")]
        public string Commentary { get; set; }

        /// <summary>
        /// The event type that caused this request to be handled
        /// </summary>
        /// <remarks>
        /// Should be "Classification Requested"
        /// </remarks>
        [JsonProperty(PropertyName = "eventType") ]
        public string EventType { get; set; }

        /// <summary>
        /// The sequence number in the command or query event stream of this request
        /// </summary>
        [JsonProperty(PropertyName = "sequenceNumber")]
        public int SequenceNumber { get; set; }

        /// <summary>
        /// The detail of what classification has been requested
        /// </summary>
        [JsonProperty(PropertyName = "eventPayload")]
        public ClassifierRequested ClassifierRequest { get; set; }

        /// <summary>
        /// The context that this request was issued for
        /// </summary>
        [JsonProperty(PropertyName = "context")]
        public WriteContext Context { get; set; }
    }
}
