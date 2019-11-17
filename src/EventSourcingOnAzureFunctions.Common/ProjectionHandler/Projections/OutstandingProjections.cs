using EventSourcingOnAzureFunctions.Common.EventSourcing;
using EventSourcingOnAzureFunctions.Common.EventSourcing.Interfaces;
using EventSourcingOnAzureFunctions.Common.ProjectionHandler.Events;
using System;
using System.Collections.Generic;
using System.Linq;

namespace EventSourcingOnAzureFunctions.Common.ProjectionHandler.Projections
{
    /// <summary>
    /// The projection to get the list of all projection requests outstanding for a
    /// given command or query
    /// </summary>
    public class OutstandingProjections
        : ProjectionBase,
        IHandleEventType<ProjectionRequested>,
        IHandleEventType<ProjectionValueReturned > 
    {

        private List<IProjectionRequest> _requestedProjections = new List<IProjectionRequest>();

        /// <summary>
        /// The set of projections that still need to be processed
        /// </summary>
        public IEnumerable<IProjectionRequest > ProjectionsToBeProcessed
        {
            get
            {
                return _requestedProjections; 
            }
        }

        public void HandleEventInstance(ProjectionRequested eventInstance)
        {
            if (! _requestedProjections.Contains(eventInstance )  )
            {
                _requestedProjections.Add(eventInstance); 
            }
        }

        public void HandleEventInstance(ProjectionValueReturned eventInstance)
        {
            if (_requestedProjections.Contains(eventInstance))
            {
                _requestedProjections.Remove(eventInstance);
            }
        }


    }
}
