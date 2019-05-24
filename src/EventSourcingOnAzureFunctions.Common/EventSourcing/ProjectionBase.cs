using EventSourcingOnAzureFunctions.Common.EventSourcing.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace EventSourcingOnAzureFunctions.Common.EventSourcing
{
    /// <summary>
    /// The base class for any projection
    /// </summary>
    public abstract class ProjectionBase
        : IProjection
    {

        

        private int _sequenceNumber = 0;
        /// <summary>
        /// The current sequence number of the last event processed by this projection
        /// </summary>
        public int CurrentSequenceNumber { get { return _sequenceNumber;  } }


        private List<ProjectionSnapshotProperty> _currentValues = new List<ProjectionSnapshotProperty>();
        public IEnumerable<ProjectionSnapshotProperty> CurrentValues => _currentValues ;

        public abstract void HandleEvent(string eventTypeName, object eventToHandle);

        public abstract bool HandlesEventType(string eventTypeName);


        public void MarkEventHandled(int handledEventSequenceNumber)
        {
            throw new NotImplementedException();
        }

        public void OnEventRead(int sequenceNumber, DateTime? asOfDate)
        {
            _sequenceNumber = sequenceNumber;
            // TODO : Process the as-of date if it is set
        }

        // Snapshots not implemented yet
        public bool SupportsSnapshots => false;

        // As of date not implemented yet
        public DateTime CurrentAsOfDate => throw new NotImplementedException();
    }
}
