using System;
using EventSourcingOnAzureFunctions.Common.Binding;
using EventSourcingOnAzureFunctions.Common.EventSourcing.Interfaces;  

namespace EventSourcingOnAzureFunctions.Common.Listener
{
    public sealed class Notification
        : IEventStreamIdentity 
    {

        /// <summary>
        /// The different types of notification that can occur
        /// </summary>
        public enum NotificationType
        {
            /// <summary>
            /// A new entity instance was created
            /// </summary>
            /// <remarks>
            /// This occurs when a new event stream is created
            /// </remarks>
            NewEntity = 1,
            /// <summary>
            /// A new event was appended to an event stream
            /// </summary>
            NewEvent = 2
        }

        /// <summary>
        /// What type of notification is this
        /// </summary>
        public NotificationType NotificationClassification { get; private set; }

        /// <summary>
        /// The name of the domain for which this event occured
        /// </summary>
        public string DomainName { get; private set; }

        /// <summary>
        /// The name of the type of entity for which this event occured
        /// </summary>
        public string EntityTypeName { get; private set; }

        /// <summary>
        /// The unique identifier of the entity for which this event occured
        /// </summary>
        public string InstanceKey { get; private set; }

        /// <summary>
        /// The business-meaningful name of the event that has occured
        /// </summary>
        /// <remarks>
        /// This will be blank for a new-entity notification
        /// </remarks>
        public string EventName { get; private set; }


        public bool MatchesFilter(NotificationType expectedNotificationType,
            string expectedDomainName,
            string expectedEntityTypeName,
            string expectedEntityKey,
            string expectedEventType)
        {
            if (expectedNotificationType != NotificationClassification )
            {
                return false;
            }

            if (! string.IsNullOrWhiteSpace(expectedDomainName ) )
            {
                if (! expectedDomainName.Equals(DomainName, StringComparison.OrdinalIgnoreCase   )  )
                {
                    return false;
                }
            }

            if (!string.IsNullOrWhiteSpace(expectedEntityTypeName))
            {
                if (!expectedEntityTypeName.Equals(EntityTypeName, StringComparison.OrdinalIgnoreCase))
                {
                    return false;
                }
            }

            if (!string.IsNullOrWhiteSpace(expectedEntityKey ))
            {
                if (!expectedEntityKey.Equals(EventTriggerAttribute.ALL_INSTANCE_KEYS , StringComparison.OrdinalIgnoreCase))
                {
                    if (!expectedEntityKey.Equals(InstanceKey , StringComparison.OrdinalIgnoreCase))
                    {
                        return false;
                    }
                }
            }

            if (expectedNotificationType == NotificationType.NewEvent  )
            {
                if (! string.IsNullOrEmpty(expectedEventType )  )
                {
                    if (!expectedEventType.Equals(EventTriggerAttribute.ALL_EVENT_TYPES, StringComparison.OrdinalIgnoreCase))
                    {
                        if (!expectedEventType.Equals(EventName , StringComparison.OrdinalIgnoreCase))
                        {
                            return false;
                        }
                    }
                }
            }

            // No mismatches found - so must be a match
            return true;
        }

        internal Notification(NotificationType notificationType ,
            IEventStreamIdentity eventStreamInstance,
            string eventName = @"")
        {
            NotificationClassification = notificationType;
            if (null != eventStreamInstance )
            {
                DomainName = eventStreamInstance.DomainName;
                EntityTypeName = eventStreamInstance.EntityTypeName;
                InstanceKey = eventStreamInstance.InstanceKey;
            }
            EventName = eventName;
        }

        public static Notification NewEntityNotification(IEventStreamIdentity eventStreamIdentity )
        {
            return new Notification(NotificationType.NewEntity,
                eventStreamIdentity); 
        }

        public static Notification NewEventNotification(IEventStreamIdentity eventStreamIdentity,
            string eventName)
        {
            return new Notification(NotificationType.NewEvent ,
                eventStreamIdentity,
                eventName );
        }

    }
}
