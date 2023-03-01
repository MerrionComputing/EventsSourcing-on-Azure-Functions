
using EventSourcingOnAzureFunctions.Common.Binding;
using Microsoft.Azure.WebJobs.Host.Executors;
using Microsoft.Azure.WebJobs.Host.Listeners;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace EventSourcingOnAzureFunctions.Common.Listener
{
    /// <summary>
    /// A listener to trigger functions as a result of command events
    /// </summary>
    public sealed class CommandListener
        : ListenerWorker, IListener
    {


        private readonly CommandStepTriggerAttribute _stepTriggerAttribute = null;

        /// <summary>
        /// Create a new command listener for the given event instance
        /// </summary>
        public CommandListener(ITriggeredFunctionExecutor executor, 
            CommandStepTriggerAttribute stepTriggerAttribute )
            : base(executor )
        {
            _stepTriggerAttribute = stepTriggerAttribute;
        }

        /// <summary>
        /// Cancel this command listener
        /// </summary>
        public void Cancel()
        {
            this.StopAsync(CancellationToken.None).ConfigureAwait(false).GetAwaiter().GetResult();
        }

        /// <summary>
        /// If this listener uses disposable objects then the IDisposable interface will need to be fully implemented
        /// </summary>
        public void Dispose()
        {
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

    }
}
