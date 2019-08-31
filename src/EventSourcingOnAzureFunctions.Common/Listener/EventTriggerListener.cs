using Microsoft.Azure.WebJobs.Host.Listeners;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace EventSourcingOnAzureFunctions.Common.Listener
{
    #if BINDING_TRIGGER
    public sealed class EventTriggerListener
        : IListener
    {

        private Task _listenerTask;
        private CancellationTokenSource _listenerStoppingTokenSource;

        /// <summary>
        /// Cancel listening for new entities being created
        /// </summary>
        public void Cancel()
        {
            StopAsync(CancellationToken.None).Wait();
        }
    }
#endif
}
