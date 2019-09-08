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
    {

        // Options to control how notifications are sent
        private readonly IOptions<EventSourcingOnAzureOptions> _options;

        // Event grid SAS key and topic connection enpoint for sending notifications via
        private readonly string eventGridKeyValue;
        private readonly string eventGridTopicEndpoint;

        // HTTP connection to be shared by all instances of this helper
        private static HttpClient httpClient = null;
        private static HttpMessageHandler httpMessageHandler = null;

        public NotificationHelper(IOptions<EventSourcingOnAzureOptions> options,
            INameResolver nameResolver)
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

            // Get the event grid instance connection details to use
            this.eventGridKeyValue = nameResolver.Resolve(options.Value.EventGridKeyValue);
            this.eventGridTopicEndpoint = options.Value.EventGridTopicEndpoint;

            if (nameResolver.TryResolveWholeString(options.Value.EventGridTopicEndpoint, out var endpoint))
            {
                this.eventGridTopicEndpoint = endpoint;
            }


            if (!string.IsNullOrEmpty(this.eventGridTopicEndpoint))
            {
                if (!string.IsNullOrEmpty(options.Value.EventGridKeySettingName))
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
        /// <param name="hubName">
        /// The name of the Event Grid hub to use to send out the notification
        /// </param>
        /// <param name="newEntity">
        /// The new entity that has been created
        /// </param>
        /// <returns></returns>
        public async Task NewEntityCreated(string hubName,
            IEventStreamIdentity newEntity)
        {

            if (this._options.Value.RaiseEntityCreationNotification    )
            {
                // TODO: Create the notification
                  
                // TODO: Send it off asynchronously

            }
            else
            {
                // Nothing to do as config doesn't want notifications sent out for new entity creation
                return;
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
}
