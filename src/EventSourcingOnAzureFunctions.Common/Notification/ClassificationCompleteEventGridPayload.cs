using EventSourcingOnAzureFunctions.Common.EventSourcing;
using EventSourcingOnAzureFunctions.Common.EventSourcing.Interfaces;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace EventSourcingOnAzureFunctions.Common.Notification
{

    public class ClassificationCompleteEventGridPayload
        : IEventStreamIdentity
    {

        public const string EVENT_TYPE = @"eventsourcing.ClassificationCompleted";
        public const string DATA_VERSION = "1.0"; // Update if the members change

        /// <summary>
        /// The notification instance identifier (for logical idempotency checking)
        /// </summary>
        [JsonProperty(PropertyName = "notificationId")]
        public string NotificationId { get; set; }

        /// <summary>
        /// The domain in which the classifier was run
        /// </summary>
        [JsonProperty(PropertyName = "domainName")]
        public string DomainName { get; set; }

        /// <summary>
        /// The type of entity against which the classifier was run 
        /// </summary>
        [JsonProperty(PropertyName = "entityTypeName")]
        public string EntityTypeName { get; set; }

        /// <summary>
        /// The unique identifier of the entity over which the classifier was run
        /// </summary>
        [JsonProperty(PropertyName = "instanceKey")]
        public string InstanceKey { get; set; }


        /// <summary>
        /// The type of the classification that was run
        /// </summary>
        [JsonProperty(PropertyName = "classificationTypeName")]
        public string ClassificationTypeName { get; set; }



        /// <summary>
        /// The sequence number of the last event read in the classification
        /// </summary>
        [JsonProperty(PropertyName = "sequenceNumber")]
        public int SequenceNumber { get; set; }

        /// <summary>
        /// The effective date of the last event read in the classification
        /// </summary>
        [JsonProperty(PropertyName = "asOfDate")]
        public Nullable<DateTime> AsOfDate { get; set; }

        /// <summary>
        /// Comments that can be passed for logging / diagnostic or other purpose
        /// </summary>
        [JsonProperty(PropertyName = "commentary")]
        public string Commentary { get; set; }


        /// <summary>
        /// Empty constructor for serialisation
        /// </summary>
        public ClassificationCompleteEventGridPayload()
        {

        }

        public static ClassificationCompleteEventGridPayload Create(IEventStreamIdentity targetEntity, 
            string classificationType, 
            int asOfSequenceNumber, 
            DateTime? asOfDate, 
            ClassificationResponse response, 
            string commentary)
        {
            throw new NotImplementedException();
        }

        public static string MakeEventTypeName(IEventStreamIdentity targetEntity, 
            string classificationType = "")
        {
            if (null == targetEntity)
            {
                return EVENT_TYPE;
            }

            return $"{targetEntity.DomainName}.{targetEntity.EntityTypeName}.{classificationType }";

        }
    }
}
