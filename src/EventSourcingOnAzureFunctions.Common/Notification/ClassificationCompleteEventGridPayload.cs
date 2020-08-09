using EventSourcingOnAzureFunctions.Common.EventSourcing;
using EventSourcingOnAzureFunctions.Common.EventSourcing.Interfaces;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace EventSourcingOnAzureFunctions.Common.Notification
{

    /// <summary>
    /// Content to send in a notification message when a classification completes
    /// </summary>
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
        /// The current result of the classification 
        /// </summary>
        [JsonProperty(PropertyName = "result")]
        public ClassificationResponse.ClassificationResults Result { get; set; }

        /// <summary>
        /// Was this entity ever included according to the classifier
        /// </summary>
        [JsonProperty(PropertyName = "wasEverIncluded")]
        public bool WasEverIncluded { get; set; }

        /// <summary>
        /// Was this entity ever excluded according to the classifier
        /// </summary>
        [JsonProperty(PropertyName = "wasEverExcluded")]
        public bool WasEverExcluded { get; set; }

        /// <summary>
        /// The parameters that were passed to the classification
        /// </summary>
        [JsonExtensionData]
        public Dictionary<string, object> Parameters { get; private set; }

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
            Dictionary<string, object> parameters,
            string notificationId = @"",
            string commentary = @"")
        {
            if (null == targetEntity)
            {
                throw new ArgumentNullException(nameof(targetEntity));
            }

            if (string.IsNullOrWhiteSpace(classificationType))
            {
                throw new ArgumentNullException(nameof(classificationType));
            }

            // Default the notification id if not are provided
            if (string.IsNullOrEmpty(notificationId))
            {
                notificationId = Guid.NewGuid().ToString("N");
            }

            ClassificationCompleteEventGridPayload ret=  new ClassificationCompleteEventGridPayload()
            {
                DomainName = targetEntity.DomainName,
                EntityTypeName = targetEntity.EntityTypeName,
                InstanceKey = targetEntity.InstanceKey,
                NotificationId = notificationId,
                Commentary = commentary,
                ClassificationTypeName = classificationType ,
                SequenceNumber = asOfSequenceNumber
            };

            if (asOfDate.HasValue)
            {
                ret.AsOfDate = asOfDate;
            }

            if (null != response)
            {
                ret.Result  = response.Result ;
                ret.WasEverExcluded = response.WasEverExcluded;
                ret.WasEverIncluded = response.WasEverIncluded;
            }

            if (null != parameters )
            {
                // add them to the response
                ret.Parameters = parameters;
            }

            return ret;
        }

        public static string MakeEventTypeName(IEventStreamIdentity targetEntity, 
            string classificationType = "")
        {
            if (null == targetEntity)
            {
                return EVENT_TYPE;
            }

            return $"{targetEntity.DomainName}.{targetEntity.EntityTypeName}.{classificationType}";

        }
    }
}
