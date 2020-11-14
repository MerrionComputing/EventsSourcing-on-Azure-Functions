using EventSourcingOnAzureFunctions.Common.CQRS.ClassifierHandler.Events;
using EventSourcingOnAzureFunctions.Common.EventSourcing;
using EventSourcingOnAzureFunctions.Common.EventSourcing.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace EventSourcingOnAzureFunctions.Common.ClassifierHandler.Projections
{
    /// <summary>
    /// The projection to get the list of all classification requests outstanding for a
    /// given command or query
    /// </summary>
    [ProjectionName("Classifications To Run") ]
    public class OutstandingClassifications
        : ProjectionBase,
        IHandleEventType<ClassifierRequested>,
        IHandleEventType<ClassifierResultReturned > 
    {

        private List<IClassifierRequest > _requestedClassifications = new List<IClassifierRequest>();

        /// <summary>
        /// The set of classifiers that still need to be processed
        /// </summary>
        public IEnumerable<IClassifierRequest> ClassificationsToBeProcessed
        {
            get
            {
                return _requestedClassifications ;
            }
        }

        public void HandleEventInstance(ClassifierRequested eventInstance)
        {
            if (!_requestedClassifications.Contains(eventInstance ) )
            {
                _requestedClassifications.Add(eventInstance);
            }
        }

        public void HandleEventInstance(ClassifierResultReturned eventInstance)
        {
            if (_requestedClassifications.Contains(eventInstance))
            {
                _requestedClassifications.Remove(eventInstance);
            }
        }


    }
}
