using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Azure.WebJobs.Host.Executors;
using Microsoft.Azure.WebJobs.Host.Listeners;

namespace EventSourcingOnAzureFunctions.Common.CQRS.Common.Listener
{

    /// <summary>
    /// A listener that tracks progress and triggers steps for a query instance
    /// </summary>
    internal sealed class CommandListener
        : CQRSListenerBase 
    {

        // Orchestration steps
        // "Who" - deciding which entities to include in the query (classifiers)
        // "What / When" - running projection over those entites
        // "What" - action to perform on those 
        // "Notify" - reply to every registered interested party (target) that the command has completed

        public CommandListener(string domainName,
            string commandName,
            string instanceIdentifier)
            : base("Command", domainName, commandName, instanceIdentifier)
        {
        }
    }
}
