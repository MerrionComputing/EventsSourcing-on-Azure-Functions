using EventSourcingOnAzureFunctions.Common.EventSourcing;
using EventSourcingOnAzureFunctions.Common.EventSourcing.Interfaces;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;

namespace EventSourcingOnAzureFunctions.Common.Notification
{
    /// <summary>
    /// Event Grid message payload for a projection completed notification
    /// </summary>
    /// <remarks>
    /// This can be used for event carried state transfer or for off-to-the-side
    /// cacheing
    /// </remarks>
    public class ProjectionCompleteEventGridPayload
        : IEventStreamIdentity 
    {

        public const string EVENT_TYPE = @"eventsourcing.ProjectionCompleted";
        public const string DATA_VERSION = "1.0"; // Update if the members change

        /// <summary>
        /// The notification instance identifier (for logical idempotency checking)
        /// </summary>
        [JsonProperty(PropertyName = "notificationId")]
        public string NotificationId { get; set; }

        /// <summary>
        /// The domain in which the projection was run
        /// </summary>
        [JsonProperty(PropertyName = "domainName")]
        public string DomainName { get; set; }

        /// <summary>
        /// The type of entity against which the projection was run 
        /// </summary>
        [JsonProperty(PropertyName = "entityTypeName")]
        public string EntityTypeName { get; set; }

        /// <summary>
        /// The unique identifier of the entity over which the projection was run
        /// </summary>
        [JsonProperty(PropertyName = "instanceKey")]
        public string InstanceKey { get; set; }

        /// <summary>
        /// The type of the projection that was run
        /// </summary>
        [JsonProperty(PropertyName = "projectionTypeName")]
        public string ProjectionTypeName { get; set; }

        /// <summary>
        /// The sequence number of the last event read in the projection
        /// </summary>
        [JsonProperty(PropertyName = "sequenceNumber")]
        public int SequenceNumber { get; set; }

        /// <summary>
        /// The effective date of the last event read in the projection
        /// </summary>
        [JsonProperty(PropertyName = "asOfDate")]
        public Nullable<DateTime>  AsOfDate { get; set; }

        /// <summary>
        /// Comments that can be passed for logging / diagnostic or other purpose
        /// </summary>
        [JsonProperty(PropertyName = "commentary")]
        public string Commentary { get; set; }

        [JsonProperty(PropertyName = "value")] 
        object CurrentValue { get; set; }

        /// <summary>
        /// Empty constructor for serialisation
        /// </summary>
        public ProjectionCompleteEventGridPayload()
        {
        }

        

        public static ProjectionCompleteEventGridPayload Create(IEventStreamIdentity targetEntity,
            string projectionType,
            int asOfSequenceNumber,
            Nullable<DateTime> asOfDate,
            object currentValue,
            string notificationId = @"",
            string commentary = @"")
        {
            if (null == targetEntity)
            {
                throw new ArgumentNullException(nameof(targetEntity));
            }

            if (string.IsNullOrWhiteSpace(projectionType))
            {
                throw new ArgumentNullException(nameof(projectionType));
            }

            // Default the notification id if not are provided
            if (string.IsNullOrEmpty(notificationId))
            {
                notificationId = Guid.NewGuid().ToString("N");
            }

            var ret = new ProjectionCompleteEventGridPayload()
            {
                DomainName = targetEntity.DomainName,
                EntityTypeName = targetEntity.EntityTypeName,
                InstanceKey = targetEntity.InstanceKey,
                NotificationId = notificationId,
                Commentary = commentary,
                ProjectionTypeName = projectionType ,
                SequenceNumber = asOfSequenceNumber
            };

            if (asOfDate.HasValue )
            {
                ret.AsOfDate = asOfDate;
            }

            if (null != currentValue)
            {
                    ret.CurrentValue = currentValue;
            }

            return ret;
        }

        public static string MakeEventTypeName(IEventStreamIdentity targetEntity, 
            string projectionType = "Anonymous Projection")
        {
            if (null == targetEntity)
            {
                return EVENT_TYPE;
            }

            return $"{targetEntity.DomainName}.{targetEntity.EntityTypeName}.{projectionType }";
        }
    }
}
