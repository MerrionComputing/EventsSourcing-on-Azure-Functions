using Microsoft.Azure.WebJobs.Host.Executors;
using Microsoft.Azure.WebJobs.Host.Listeners;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace EventSourcingOnAzureFunctions.Common.Listener
{
    #if BINDING_TRIGGER
    /// <summary>
    /// A class that listens out for a new entity being created and if one occurs triggers all the 
    /// relevant functions with their [NewEntityTrigger]
    /// </summary>
    public sealed class NewEntityTriggerListener
        : IListener
    {

        private Task _listenerTask;
        private CancellationTokenSource _listenerStoppingTokenSource;
        private readonly ITriggeredFunctionExecutor _executor;

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
            _listenerStoppingTokenSource = new CancellationTokenSource();
            _listenerTask = ListenAsync(_listenerStoppingTokenSource.Token);

            return _listenerTask.IsCompleted ? _listenerTask : Task.CompletedTask;
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            if (_listenerTask == null)
            {
                return;
            }

            try
            {
                _listenerStoppingTokenSource.Cancel();
            }
            finally
            {
                await Task.WhenAny(_listenerTask,
                    Task.Delay(Timeout.Infinite, cancellationToken));
            }

        }

        private async Task ListenAsync(CancellationToken listenerStoppingToken)
        {

            while (!listenerStoppingToken.IsCancellationRequested)
            {
                while (await GetNextNewEntityCreationEvent())
                {
                    await _executor.TryExecuteAsync(
                            new TriggeredFunctionData() { TriggerValue = changefeed.Current },
                            CancellationToken.None
                        );
                }
            }
        }

        public NewEntityTriggerListener(ITriggeredFunctionExecutor executor)
        {
            _executor = executor;
        }

#region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects).
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.

                disposedValue = true;
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        // ~NewEntityTriggerListener()
        // {
        //   // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
        //   Dispose(false);
        // }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            // TODO: uncomment the following line if the finalizer is overridden above.
            // GC.SuppressFinalize(this);
        }
#endregion

    }
#endif
}
