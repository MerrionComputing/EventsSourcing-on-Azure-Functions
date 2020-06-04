using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Options;
using System;
using System.Net.Http;
using Microsoft.Azure.WebJobs.Host;
using System.Net;
using System.Threading.Tasks;
using System.Runtime.ExceptionServices;
using System.Linq;
using System.Threading;
using EventSourcingOnAzureFunctions.Common.EventSourcing.Interfaces;
using Microsoft.Azure.EventGrid.Models;
using Newtonsoft.Json;
using System.Text;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using EventSourcingOnAzureFunctions.Common.EventSourcing;

namespace EventSourcingOnAzureFunctions.Common.Notification
{
    /// <summary>
    /// Common routines for sending out notifications via event grid when an event sourcing 
    /// event has occured
    /// </summary>
    /// <remarks> 
    /// Notifications are raised when a new entity is created (i.e. when a new event strem is created) and
    /// when a new evenmt is appended to an event stream
    /// </remarks>
    public sealed class NotificationHelper
        : INotificationDispatcher
    {

        // Options to control how notifications are sent
        private readonly IOptions<EventSourcingOnAzureOptions> _options;
        private readonly ILogger _logger;

        // Event grid SAS key and topic connection enpoint for sending notifications via
        private readonly string eventGridKeyValue;
        private readonly string eventGridTopicEndpoint;

        // HTTP connection to be shared by all instances of this helper
        private static HttpClient httpClient = null;
        private static HttpMessageHandler httpMessageHandler = null;

        // trade http header constants
        public const string TRACE_HEADER_PARENT = "traceparent";
        public const string TRACE_HEADER_STATE = "tracestate";

        public NotificationHelper(IOptions<EventSourcingOnAzureOptions> options,
            INameResolver nameResolver,
            ILogger logger )
        {

            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }


            if (nameResolver == null)
            {
                throw new ArgumentNullException(nameof(nameResolver));
            }

            _options = options;

            if (null != logger )
            {
                _logger = logger;
            }

            // Get the event grid instance connection details to use
            this.eventGridKeyValue = options.Value.EventGridKeyValue;
            this.eventGridTopicEndpoint = options.Value.EventGridTopicEndpoint;

            if (nameResolver.TryResolveWholeString(options.Value.EventGridTopicEndpoint, out var endpoint))
            {
                this.eventGridTopicEndpoint = endpoint;
            }

            if (nameResolver.TryResolveWholeString(options.Value.EventGridKeyValue, out var keyvalue))
            {
                this.eventGridKeyValue = keyvalue;
            }

            if (!string.IsNullOrEmpty(this.eventGridTopicEndpoint))
            {
                    // The HTTP responses that are not fatal and deserve a retry
                    // (NOT HttpStatusCode.ServiceUnavailable)
                    HttpStatusCode[] retryStatusCode = new HttpStatusCode[]{
                            HttpStatusCode.RequestTimeout ,
                            HttpStatusCode.InternalServerError ,
                            HttpStatusCode.GatewayTimeout ,
                            HttpStatusCode.NetworkAuthenticationRequired
                        };

                    // Start the message handler, with these retry statuses set
                    this.HttpMessageHandler = new HttpRetryMessageHandler(
                        new HttpClientHandler(),
                        options.Value.EventGridPublishRetryCount,
                        options.Value.EventGridPublishRetryInterval,
                        retryStatusCode);
            }
        }

        public string EventGridKeyValue => this.eventGridKeyValue;

        public string EventGridTopicEndpoint => this.eventGridTopicEndpoint;


        public HttpMessageHandler HttpMessageHandler
        {
            get => httpMessageHandler;
            set
            {
                httpClient?.Dispose();
                httpMessageHandler = value;
                httpClient = new HttpClient(httpMessageHandler);
                httpClient.DefaultRequestHeaders.Add("aeg-sas-key", this.eventGridKeyValue);
            }
        }

        /// <summary>
        /// A new entity was created - notify the world
        /// </summary>
        /// <param name="newEntity">
        /// The new entity that has been created
        /// </param>
        public async Task NewEntityCreated(IEventStreamIdentity newEntity,
            string commentary = @"",
            IWriteContext context = null)
        {

            if (this._options.Value.RaiseEntityCreationNotification)
            { 

                // Create the notification
                NewEntityEventGridPayload payload = NewEntityEventGridPayload.Create(newEntity,
                    commentary: commentary ,
                    context: context  );

                // Create an event grid message to send
                EventGridEvent[] message = new EventGridEvent[]
                {
                    new EventGridEvent()
                    {
                        Id = Guid.NewGuid().ToString(),
                        EventType = NewEntityEventGridPayload.MakeEventTypeName(newEntity ) ,
                        Subject = MakeEventGridSubject(newEntity) ,
                        DataVersion = NewEntityEventGridPayload.DATA_VERSION,
                        Data = payload,
                        EventTime = DateTime.UtcNow  
                    }
                };

                // get any context to add as headers
                string correlationId = "";
                string causationId = "";

                if (context != null)
                {
                    correlationId = context.CorrelationIdentifier;
                    causationId = context.CausationIdentifier;
                }

                // Send it off asynchronously
                await SendNotificationAsync(message, correlationId, causationId );
            }
            else
            {
                // Nothing to do as config doesn't want notifications sent out for new entity creation
                return;
            }
        }

        /// <summary>
        /// An entity was deleyed - notify the world
        /// </summary>
        /// <param name="newEntity">
        /// The new entity that has been created
        /// </param>
        public async Task ExistingEntityDeleted(IEventStreamIdentity deletedEntity, 
            string commentary = "", 
            IWriteContext context = null)
        {
            if (this._options.Value.RaiseEntityCreationNotification)
            {

                // Create the notification
                DeletedEntityEventGridPayload payload = DeletedEntityEventGridPayload.Create(deletedEntity,
                    commentary: commentary,
                    context: context);

                // Create an event grid message to send
                EventGridEvent[] message = new EventGridEvent[]
                {
                    new EventGridEvent()
                    {
                        Id = Guid.NewGuid().ToString(),
                        EventType = DeletedEntityEventGridPayload.MakeEventTypeName(deletedEntity ) ,
                        Subject = MakeEventGridSubject(deletedEntity) ,
                        DataVersion = DeletedEntityEventGridPayload.DATA_VERSION,
                        Data = payload,
                        EventTime = DateTime.UtcNow
                    }
                };

                // get any context to add as headers
                string correlationId = "";
                string causationId = "";

                if (context != null)
                {
                    correlationId = context.CorrelationIdentifier;
                    causationId = context.CausationIdentifier;
                }

                // Send it off asynchronously
                await SendNotificationAsync(message, correlationId, causationId );
            }
            else
            {
                // Nothing to do as config doesn't want notifications sent out for new entity creation
                return;
            }
        }


        /// <summary>
        /// A new event was appended to an event stream - notify the world
        /// </summary>
        /// <param name="targetEntity">
        /// The entity on which event stream the event was appended
        /// </param>
        /// <param name="eventType">
        /// The type of event that occured
        /// </param>
        /// <param name="sequenceNumber">
        /// The sequence number of the new event that was appended
        /// </param>
        /// <returns></returns>
        public async Task NewEventAppended(IEventStreamIdentity targetEntity,
            string eventType,
            int sequenceNumber,
            string commentary = @"",
            object eventPayload = null,
            IWriteContext context = null)
        {

            if (this._options.Value.RaiseEventNotification)
            {

                // Create the notification
                NewEventEventGridPayload payload = NewEventEventGridPayload.Create(targetEntity,
                    eventType,
                    sequenceNumber,
                    commentary: commentary ,
                    eventPayload:  eventPayload,
                    context: context );

                // Create an event grid message to send
                EventGridEvent[] message = new EventGridEvent[]
                {
                    new EventGridEvent()
                    {
                        Id = Guid.NewGuid().ToString(),
                        EventType = NewEventEventGridPayload.MakeEventTypeName(targetEntity, eventType )   ,
                        Subject = MakeEventGridSubject(targetEntity, eventType) ,
                        DataVersion = NewEventEventGridPayload.DATA_VERSION ,
                        Data = payload,
                        EventTime = DateTime.UtcNow
                    }
                };

                // get any context to add as headers
                string correlationId = "";
                string causationId = "";

                if (context != null)
                {
                    correlationId = context.CorrelationIdentifier;
                    causationId = context.CausationIdentifier;
                }

                // Send it off asynchronously
                await SendNotificationAsync(message, correlationId , causationId );
            }
            else
            {
                // Nothing to do as config doesn't want notifications sent out for new entity creation
                return;
            }
        }

        /// <summary>
        /// Send a notification that a projection has been run
        /// </summary>
        /// <param name="targetEntity">
        /// The entity instance over which the projection was run
        /// </param>
        /// <param name="projectionType">
        /// The type of the projection that was run
        /// </param>
        /// <param name="asOfSequenceNumber">
        /// The sequence number of the last event in the stream that was read in this projection
        /// </param>
        /// <param name="asOfDate">
        /// (Optional) The as-of date passed to the projection request
        /// </param>
        /// <param name="currentValue">
        /// The value for the state as read by the projection
        /// </param>
        /// <param name="commentary">
        /// (Optional) Additional commentary
        /// </param>
        public async Task ProjectionCompleted(IEventStreamIdentity targetEntity,
            string projectionType,
            int asOfSequenceNumber,
            DateTime? asOfDate,
            object currentValue,
            string commentary = "")
        {
            if (this._options.Value.RaiseProjectionCompletedNotification)
            {
                //  Create the notification
                ProjectionCompleteEventGridPayload payload = ProjectionCompleteEventGridPayload.Create(
                    targetEntity ,
                    projectionType,
                    asOfSequenceNumber ,
                    asOfDate ,
                    currentValue,
                    commentary 
                    );


                // Create an event grid message to send
                EventGridEvent[] message = new EventGridEvent[]
                {
                    new EventGridEvent()
                    {
                        Id = Guid.NewGuid().ToString(),
                        EventType = ProjectionCompleteEventGridPayload.MakeEventTypeName(targetEntity, projectionType  )   ,
                        Subject = MakeEventGridSubject(targetEntity, projectionType ) ,
                        DataVersion = NewEventEventGridPayload.DATA_VERSION ,
                        Data = payload,
                        EventTime = DateTime.UtcNow
                    }
                };

                // Send it off asynchronously
                await SendNotificationAsync(message);
            }
            else
            {
                // Nothing to do as config doesn't want notifications sent out for 
                // projection completion
                return;
            }
        }


        /// <summary>
        /// A classification completed
        /// </summary>
        /// <param name="targetEntity">
        /// The entity over which the classification process was run
        /// </param>
        /// <param name="classificationType">
        /// The type of the classification
        /// </param>
        /// <param name="parameters">
        /// Any additional parameters used when processing the classification
        /// </param>
        /// <param name="asOfSequenceNumber">
        /// The sequence number of the last event read in processing the classification
        /// </param>
        /// <param name="asOfDate">
        /// The as-of date up to which the classification was ran
        /// </param>
        /// <param name="commentary">
        /// (Optional) Additional commentary to pass with the notification
        /// </param>
        public async Task ClassificationCompleted(IEventStreamIdentity targetEntity, 
            string classificationType, 
            Dictionary<string, object> parameters, 
            int asOfSequenceNumber, 
            DateTime? asOfDate, 
            ClassificationResponse response, 
            string commentary = "")
        {
            if (this._options.Value.RaiseClassificationCompletedNotification )
            {
                //  Create the notification
                ClassificationCompleteEventGridPayload payload = ClassificationCompleteEventGridPayload.Create(
                    targetEntity,
                    classificationType,
                    asOfSequenceNumber,
                    asOfDate,
                    response,
                    parameters,
                    commentary
                    );


                // Create an event grid message to send
                EventGridEvent[] message = new EventGridEvent[]
                {
                    new EventGridEvent()
                    {
                        Id = Guid.NewGuid().ToString(),
                        EventType = ClassificationCompleteEventGridPayload.MakeEventTypeName(targetEntity, 
                        classificationType  )   ,
                        Subject = MakeEventGridSubject(targetEntity, classificationType ) ,
                        DataVersion = NewEventEventGridPayload.DATA_VERSION ,
                        Data = payload,
                        EventTime = DateTime.UtcNow
                    }
                };

                // Send it off asynchronously
                await SendNotificationAsync(message);
            }
            else
            {
                // Nothing to do as config doesn't want notifications sent out for 
                // classification completion
                return;
            }
        }

        /// <summary>
        /// Turn an entity identifier into an eventgrid message subject
        /// </summary>
        /// <param name="newEntity">
        /// The entity the message is being sent about
        /// </param>
        /// <param name="eventType">
        /// The name of the event type that we are raising a notification for
        /// </param>
        /// <remarks>
        /// If an event type is specified this will come in front of the instance identifier as that is
        /// the more useful pattern for filtering
        /// </remarks>
        public static string MakeEventGridSubject(IEventStreamIdentity newEntity,
            string eventType = "")
        {
            if (string.IsNullOrWhiteSpace(eventType ))
            {
                return $"eventsourcing/{MakeEventGridSubjectPart(newEntity.DomainName)}/{newEntity.EntityTypeName}/{newEntity.InstanceKey}";
            }
            else
            {
                return $"eventsourcing/{MakeEventGridSubjectPart(newEntity.DomainName)}/{newEntity.EntityTypeName}/{eventType}/{newEntity.InstanceKey}";
            }
        }

        /// <summary>
        /// Split a multi-part subject part with path separators
        /// </summary>
        /// <param name="subjectPart">
        /// The original subject part with dot separators
        /// </param>
        public static string MakeEventGridSubjectPart(string subjectPart)
        {
            return subjectPart.Replace(".", "/");  
        }

        private async Task SendNotificationAsync(
                EventGridEvent[] eventGridEventArray,
                string correlationIdentifier = "",
                string causationIdentifier = "")
        {
            string json = JsonConvert.SerializeObject(eventGridEventArray);
            StringContent content = new StringContent(json, Encoding.UTF8, "application/json");
            HttpResponseMessage result = null;
            try
            {
                if ( (!string.IsNullOrWhiteSpace(correlationIdentifier ) ) ||
                    (!string.IsNullOrWhiteSpace(causationIdentifier )))
                {
                    // Add a W3C Trace header
                    content.Headers.Add(TRACE_HEADER_PARENT, MakeTraceParent(correlationIdentifier, causationIdentifier));
                }
                result = await httpClient.PostAsync(this.eventGridTopicEndpoint, content);
            }
            catch (Exception e)
            {
                if (null != _logger )
                {
                    _logger.LogError(e.Message);  
                }
                return;
            }

            using (result)
            {
                var body = await result.Content.ReadAsStringAsync();
                if (result.IsSuccessStatusCode)
                {
                    // Successfully sent the notification...
                    if (null != _logger)
                    {
                        _logger.LogInformation($"Sent notification via {this.eventGridTopicEndpoint}" );
                    }
                }
                else
                {
                    // Failed to send the eventgrid notification...
                    if (null != _logger)
                    {
                        _logger.LogError($"Failed to send notification - {result.StatusCode} {result.Content}");
                    }
                }
            }
        }


        internal class HttpRetryMessageHandler :
            DelegatingHandler
        {
            public HttpRetryMessageHandler(HttpMessageHandler messageHandler,
                int maxRetryCount,
                TimeSpan retryWaitSpan,
                HttpStatusCode[] retryTargetStatusCode)
                : base(messageHandler)
            {
                this.MaxRetryCount = maxRetryCount;
                this.RetryWaitSpan = retryWaitSpan;
                this.RetryTargetStatus = retryTargetStatusCode;
            }

            public int MaxRetryCount { get; }

            public TimeSpan RetryWaitSpan { get; }

            public HttpStatusCode[] RetryTargetStatus { get; }

            protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,
                CancellationToken cancellationToken)
            {
                var tryCount = 0;
                Exception lastException = null;
                HttpResponseMessage response = null;
                do
                {
                    try
                    {
                        response = await base.SendAsync(request, cancellationToken);
                        if (response.IsSuccessStatusCode)
                        {
                            return response;
                        }
                        else if (response.StatusCode != HttpStatusCode.ServiceUnavailable)
                        {
                            if (this.RetryTargetStatus.All(x => x != response.StatusCode))
                            {
                                return response;
                            }
                        }
                    }
                    catch (HttpRequestException e)
                    {
                        lastException = e;
                    }

                    tryCount++;

                    await Task.Delay(this.RetryWaitSpan, cancellationToken);
                }
                while (this.MaxRetryCount >= tryCount);

                if (response != null)
                {
                    return response;
                }
                else
                {
                    ExceptionDispatchInfo.Capture(lastException).Throw();
                    return null;
                }
            }
        }

        /// <summary>
        /// Make a string that can be passed to the W3C Trace header as a trace paren
        /// </summary>
        /// <param name="correlationIdentifier">
        /// The string we used as our correlation identifier 
        /// </param>
        /// <param name="causationIdentifier">
        /// The string we used as our causation identifier
        /// </param>
        /// <returns>
        /// 
        /// </returns>
        /// <remarks>
        /// See https://www.w3.org/TR/trace-context/#examples-of-http-traceparent-headers
        /// </remarks>
        public static string MakeTraceParent(string correlationIdentifier, string causationIdentifier)
        {

            string version = "00";
            string traceFlags = "00"; //not sampled

            if (! string.IsNullOrWhiteSpace(correlationIdentifier ) )
            {
                // turn it into an 16-byte array of lowercase hex
                correlationIdentifier = StringToByteArray(16, correlationIdentifier);
            }

            if (! string.IsNullOrWhiteSpace(causationIdentifier ) )
            {
                // turn it into an 8-byte array of lowercase hex
                causationIdentifier = StringToByteArray(8, causationIdentifier);
            }

            // If not able to make a header, return a "null" one
            return $"{version}-00000000000000000000000000000000-0000000000000000-{traceFlags}";
        }


        /// <summary>
        /// Turn a string into a byte array to make a correlation/causation ID useful for W3C tracing
        /// </summary>
        /// <param name="length">
        /// The length of array we need
        /// </param>
        /// <param name="input">
        /// The source string we are turning into an array
        /// </param>
        public static string StringToByteArray(int length, string input)
        {
            if (length <= 0)
            {
                throw new ArgumentException("Length must be greater than zero"); 
            }

            if (! string.IsNullOrWhiteSpace(input ) )
            {

            }

            return new string('0', length);
            
        }
    }
}