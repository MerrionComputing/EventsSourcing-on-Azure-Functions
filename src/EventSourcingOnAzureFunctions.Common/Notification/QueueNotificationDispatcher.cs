using EventSourcingOnAzureFunctions.Common.EventSourcing;
using EventSourcingOnAzureFunctions.Common.EventSourcing.Interfaces;
using EventSourcingOnAzureFunctions.Common.CQRS.ProjectionHandler.Events;
using EventSourcingOnAzureFunctions.Common.CQRS.ClassifierHandler.Events;
using EventSourcingOnAzureFunctions.Common.EventSourcing.Implementation.AzureStorage.AppendBlob;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using System.IO;
using Azure.Storage.Queues;

namespace EventSourcingOnAzureFunctions.Common.Notification
{
    /// <summary>
    /// Common routines for sending out notifications via named queues when an event sourcing 
    /// event has occured
    /// </summary>
    /// <remarks> 
    /// Notifications are raised when a new entity is created (i.e. when a new event strem is created) and
    /// when a new event is appended to an event stream.
    /// Due to the small size of messages that can be put on queues (64kb) these notifications are
    /// not used for event carried state transfer so have no payload.
    /// </remarks>
    public sealed class QueueNotificationDispatcher
        : INotificationDispatcher
    {

        private readonly IEventStreamSettings _eventStreamSettings;

        // strings representing the notification types, 
        public const string NOTIFICATION_NEW_ENTITY = "N";
        public const string NOTIFICATION_NEW_EVENT = "E";
        public const string NOTIFICATION_ENTITY_DELETED = "D";
        public const string NOTIFICATION_PROJECTION = "P";
        public const string NOTIFICATION_CLASSIFICATION = "C";

        // Special queue names for CQRS processing
        public const string QUEUE_QUERY_PROJECTIONS = "query-projection-requests";
        public const string QUEUE_QUERY_CLASSIFICATIONS = "query-classification-requests";
        public const string QUEUE_COMMAND_PROJECTIONS = "command-projection-requests";
        public const string QUEUE_COMMAND_CLASSIFICATIONS = "query-classification-requests";

        // Options to control how notifications are sent
        private readonly IOptions<EventSourcingOnAzureOptions> _options;
        private readonly ILogger _logger;


        /// <summary>
        /// The name by which this notification dispatcher is known
        /// </summary>
        public string Name => nameof(QueueNotificationDispatcher);

        public async Task NewEntityCreated(IEventStreamIdentity newEntity, 
            string commentary = "", 
            IWriteContext context = null)
        {
            string messageToSend = MakeMessageString(newEntity ,
                    NOTIFICATION_NEW_ENTITY,
                    "",
                    0);

            string queueName = MakeQueueName(newEntity);

            string connectionStringName = _eventStreamSettings.GetConnectionStringName(newEntity);

            if (!string.IsNullOrWhiteSpace(queueName))
            {
                await SendQueueMessage(connectionStringName, queueName, messageToSend);
            }
        }

        public async Task NewEventAppended(IEventStreamIdentity targetEntity, 
            string eventType, 
            int sequenceNumber, 
            string commentary = "", 
            object eventPayload = null, 
            IWriteContext context = null)
        {

            string messageToSend = MakeMessageString(targetEntity,
                NOTIFICATION_NEW_EVENT,
                eventType,
                sequenceNumber);

            string queueName = string.Empty ; 
            // special case - if it is a Command or Query requesting a Classification or Projection..
            if (eventType == EventNameAttribute.GetEventName(typeof(ProjectionRequested) ))
            {
                // Is it a command or query...
                if (targetEntity.DomainName.Contains("Command"))
                {
                    queueName = QUEUE_COMMAND_PROJECTIONS;
                }
                if (targetEntity.DomainName.Contains("Query"))
                {
                    queueName = QUEUE_QUERY_PROJECTIONS;
                }
                // Add the extra details to the message text to indicate what projection was requested
                ProjectionRequested evtPayload = eventPayload as ProjectionRequested;
                if (evtPayload != null)
                {
                    messageToSend += ProjectionRequested.ToQueueMessage(evtPayload);
                }
            }
            else
            {
                if (eventType == EventNameAttribute.GetEventName(typeof(ClassifierRequested)))
                {
                    // Is it a command or query...
                    if (targetEntity.DomainName.Contains("Command"))
                    {
                        queueName = QUEUE_COMMAND_CLASSIFICATIONS;
                    }
                    if (targetEntity.DomainName.Contains("Query"))
                    {
                        queueName = QUEUE_QUERY_CLASSIFICATIONS;
                    }
                    // Add the extra details to the message text to indicate what classification was requested
                    ClassifierRequested evtPayload = eventPayload as ClassifierRequested;
                    if (evtPayload != null)
                    {
                        messageToSend += $"|{evtPayload.DomainName}|{evtPayload.EntityTypeName}|{evtPayload.InstanceKey}|{evtPayload.ClassifierTypeName}|{evtPayload.AsOfDate}|{evtPayload.CorrelationIdentifier}";
                    }
                }
                else
                {
                    queueName  = MakeQueueName(targetEntity); 
                }
            }

            string connectionStringName = _eventStreamSettings.GetConnectionStringName(targetEntity);

            if (! string.IsNullOrWhiteSpace(queueName ))
            {
                await SendQueueMessage(connectionStringName, queueName, messageToSend);
            }

        }



        public async Task ExistingEntityDeleted(IEventStreamIdentity deletedEntity, 
            string commentary = "", 
            IWriteContext context = null)
        {

            string messageToSend = MakeMessageString(deletedEntity,
                NOTIFICATION_ENTITY_DELETED,
                "",
                0);

            string queueName = MakeQueueName(deletedEntity);

            string connectionStringName = _eventStreamSettings.GetConnectionStringName(deletedEntity);

            if (!string.IsNullOrWhiteSpace(queueName))
            {
                await SendQueueMessage(connectionStringName, queueName, messageToSend);
            }
        }


        public async Task ProjectionCompleted(IEventStreamIdentity targetEntity, 
            string projectionType, 
            int asOfSequenceNumber, 
            DateTime? asOfDate, 
            object currentValue, 
            string commentary = "")
        {

            string messageToSend = MakeMessageString(targetEntity,
                NOTIFICATION_PROJECTION,
                projectionType ,
                asOfSequenceNumber,
                asOfDate );

            string queueName = MakeQueueName(targetEntity);

            string connectionStringName = _eventStreamSettings.GetConnectionStringName(targetEntity);

            if (!string.IsNullOrWhiteSpace(queueName))
            {
                await SendQueueMessage(connectionStringName, queueName, messageToSend);
            }

        }

        public async Task ClassificationCompleted(IEventStreamIdentity targetEntity, 
            string classificationType, 
            Dictionary<string, object> parameters, 
            int asOfSequenceNumber, 
            DateTime? asOfDate, 
            ClassificationResponse response, 
            string commentary = "")
        {

            string messageToSend = MakeMessageString(targetEntity,
                    NOTIFICATION_CLASSIFICATION,
                    classificationType ,
                    asOfSequenceNumber ,
                    asOfDate );

            string queueName = MakeQueueName(targetEntity);

            string connectionStringName = _eventStreamSettings.GetConnectionStringName(targetEntity);

            if (!string.IsNullOrWhiteSpace(queueName))
            {
                await SendQueueMessage(connectionStringName, queueName, messageToSend);
            }
        }

        /// <summary>
        /// Send the given message to the specified queue 
        /// </summary>
        /// <param name="connectionStringName">
        /// The name of the connection string to use to connect to the queue
        /// </param>
        /// <param name="queueName">
        /// The name of the queue to post the message to
        /// </param>
        /// <param name="messageToSend">
        /// The message to put on the queue
        /// </param>
        /// <remarks>
        /// If the queue does not exist it will be created
        /// </remarks>
        private async Task SendQueueMessage(string connectionStringName, 
            string queueName, 
            string messageToSend)
        {
            // get the connection string named...
            if (!string.IsNullOrWhiteSpace(connectionStringName))
            {
                // Get the connection string for the name
                // Create a connection to the cloud storage account to use
                ConfigurationBuilder builder = new ConfigurationBuilder();
                builder.SetBasePath(Directory.GetCurrentDirectory())
                    .AddJsonFile("appsettings.json", true)
                    .AddJsonFile("local.settings.json", true)
                    .AddJsonFile("config.local.json", true)
                    .AddJsonFile("config.json", true)
                    .AddJsonFile("connectionstrings.json", true)
                    .AddEnvironmentVariables();

                IConfigurationRoot config = builder.Build();

                string connectionString = config.GetConnectionString(connectionStringName);
                if (string.IsNullOrWhiteSpace(connectionString))
                {
                    throw new NullReferenceException($"No connection string configured for {connectionStringName}");
                }

                QueueClient queueClient = new QueueClient(connectionString  , queueName);

                // Create the queue if it does not exist
                await queueClient.CreateIfNotExistsAsync();

                // and send the message
                await queueClient.SendMessageAsync(messageToSend);

            }
        }

        public QueueNotificationDispatcher(IOptions<EventSourcingOnAzureOptions> options,
            IEventStreamSettings settings,
            ILogger logger)
        {

            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            _options = options;

            if (settings  == null)
            {
                throw new ArgumentNullException(nameof(settings));
            }

            _eventStreamSettings = settings;

            if (null != logger)
            {
                _logger = logger;
            }


        }

        /// <summary>
        /// Turn the identity of an event stream backed entity into the name of a 
        /// queue down which the notifications about that entity should be posted
        /// </summary>
        /// <param name="targetEntity">
        /// The entity for which a notification has occurred
        /// </param>
        /// <returns></returns>
        public static string MakeQueueName(IEventStreamIdentity targetEntity)
        {
            if (targetEntity != null)
            {
                string basename= BlobEventStreamBase.MakeValidStorageFolderName($"{targetEntity.DomainName}-{targetEntity.EntityTypeName}");
                // Queues also cannot start with a number
                return basename.TrimStart(new char[] { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9' });
            }

            // The target that applies if the entity cannot be identified
            return $"unidentified-entities";
        }

        /// <summary>
        /// Turn the properties passed in into a string to be sent via the queue
        /// </summary>
        /// <param name="targetEntity">
        /// What the notification occured for
        /// </param>
        /// <param name="NotificationType">
        /// The type of notification that occured 
        /// </param>
        /// <param name="asOfSequenceNumber">
        /// The sequence number of the event stream for which this notification occured
        /// </param>
        /// <param name="asOfDate">
        /// The as-of date of the event in the event stream for which this notification occured
        /// </param>
        /// <returns>
        /// A notification message less than 64kb long as pipe-separated values
        /// </returns>
        public static string MakeMessageString(IEventStreamIdentity targetEntity,
            string NotificationType,
            string NotificationClass,
            int asOfSequenceNumber,
            DateTime? asOfDate = null
            )
        {
            if (asOfDate.HasValue)
            {
                return $"{NotificationType}|{NotificationClass}|{targetEntity.InstanceKey}|{asOfSequenceNumber}|{asOfDate}";
            }
            return $"{NotificationType}|{NotificationClass}|{targetEntity.InstanceKey}|{asOfSequenceNumber}|null";
        }

    }
}
