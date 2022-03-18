using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Azure.WebJobs.Host.Executors;
using Microsoft.Azure.WebJobs.Host.Listeners;

namespace EventSourcingOnAzureFunctions.Common.CQRS.Common.Listener
{
    /// <summary>
    /// A common base class for all the CQRS listeners
    /// </summary>
    /// <remarks>
    /// This is done by out-of-process
    /// </remarks>
    internal class CQRSListenerBase
        : IListener
    {

        private readonly string _listenerType; // Query, Command, Projection, Classifier etc.
        public readonly string _domainName; // Domain the command/query etc is running in
        public readonly string _name; // The name of the command, query etc.
        public readonly string _instanceIdentifier; // the unique instance identifier of the command, query etc.



        public void Cancel()
        {
            this.StopAsync(CancellationToken.None).ConfigureAwait(false).GetAwaiter().GetResult();
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            if (! string.IsNullOrWhiteSpace(_listenerType ))
            {

            }
            throw new NotImplementedException();
        }


        internal CQRSListenerBase(string listenerType,
            string domainName,
            string name,
            string instanceIdentifier)
        {
            _listenerType = listenerType;
            _domainName = domainName;
            _name = name;
            _instanceIdentifier = instanceIdentifier;
        }
    }
}
