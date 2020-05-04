using EventSourcingOnAzureFunctions.Common.CQRS.QueryHandler.Events;
using EventSourcingOnAzureFunctions.Common.EventSourcing;
using EventSourcingOnAzureFunctions.Common.EventSourcing.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace EventSourcingOnAzureFunctions.Common.CQRS.QueryHandler.Projections
{
    /// <summary>
    /// The set of all the output locations for a query results to be sent to
    /// </summary>
    public sealed class OutputLocations
        : ProjectionBase,
        IHandleEventType<OutputLocationSet>
    {

        private List<OutputLocationSet> _requestedOutputs = new List<OutputLocationSet>();

        /// <summary>
        /// The locations the output of this query should be setnt
        /// </summary>
        public IEnumerable<OutputLocationSet> RequestedOutputs
        {
            get
            {
                return _requestedOutputs ;
            }
        }

        public void HandleEventInstance(OutputLocationSet eventInstance)
        {
            if (! _requestedOutputs.Contains(eventInstance))
            {
                _requestedOutputs.Add(eventInstance);
            }
        }
    }
}
