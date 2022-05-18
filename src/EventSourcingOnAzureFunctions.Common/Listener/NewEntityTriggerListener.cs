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
    /// A class that listens out for a new entity being created and if one occurs triggers all the 
    /// relevant functions with their [NewEntityTrigger]
    /// </summary>
    public sealed class NewEntityTriggerListener
        : ListenerWorker, IListener
    {


        private readonly NewEntityTriggerAttribute _newEntityTrigger;

        /// <summary>
        /// Cancel listening for new entities being created
        /// </summary>
        public void Cancel()
        {
            StopAsync(CancellationToken.None).Wait();
        }

        /// <summary>
        /// Start the listener process that waits for new entity events to tell the world about
        /// </summary>
        public Task StartAsync(CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }

        public NewEntityTriggerListener(ITriggeredFunctionExecutor executor,  NewEntityTriggerAttribute newEntityTrigger)
            : base(executor)
        {
            _newEntityTrigger = newEntityTrigger;
        }

    }

}
