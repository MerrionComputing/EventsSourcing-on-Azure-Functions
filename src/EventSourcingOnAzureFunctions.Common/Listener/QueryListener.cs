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
    /// A listener to trigger azure functions for query events
    /// </summary>
    public sealed class QueryListener
        :  ListenerWorker, IListener
    {
        private readonly QueryTriggerAttribute _queryTrigger;



        /// <summary>
        /// Create a new query event listener for the given query instance
        /// </summary>
        public QueryListener(ITriggeredFunctionExecutor executor, QueryTriggerAttribute queryTrigger)
            : base(executor)
        {
            _queryTrigger = queryTrigger;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public void Cancel()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// If this listener uses disposable objects then the IDisposable interface will need to be fully implemented
        /// </summary>
        public void Dispose()
        {
        }
    }
}
