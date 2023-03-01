using EventSourcingOnAzureFunctions.Common.Binding;
using Microsoft.Azure.WebJobs.Host.Executors;
using Microsoft.Azure.WebJobs.Host.Listeners;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace EventSourcingOnAzureFunctions.Common.Listener
{
    /// <summary>
    /// A listener to fire off an Azure function whenever a matching event is written to the 
    /// event stream
    /// </summary>
    /// <remarks>
    /// This will only trigger functions in the same function app - all intra domain communtication
    /// should be performed by Azure EventGrid
    /// </remarks>
    public sealed class EventTriggerListener
        : ListenerWorker, IListener
    {

        private readonly EventTriggerAttribute _eventTrigger;

        /// <summary>
        /// Cancel listening for new entities being created
        /// </summary>
        public void Cancel()
        {
            StopAsync(CancellationToken.None).Wait();
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// If this listener uses disposable objects then the IDisposable interface will need to be fully implemented
        /// </summary>
        public void Dispose()
        {
        }

        public EventTriggerListener(ITriggeredFunctionExecutor executor, EventTriggerAttribute eventTrigger)
            : base(executor )
        {
            _eventTrigger = eventTrigger;
        }
    }
}
