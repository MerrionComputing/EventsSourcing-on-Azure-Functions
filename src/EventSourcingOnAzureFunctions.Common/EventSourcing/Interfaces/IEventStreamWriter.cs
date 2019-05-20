using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace EventSourcingOnAzureFunctions.Common.EventSourcing.Interfaces
{
    public interface IEventStreamWriter
        : IEventStreamIdentity
    {


        /// <summary>
        /// Save an event onto the end of the event stream
        /// </summary>
        /// <param name="eventInstance">
        /// The specific event to append to the end of the store
        /// </param>
        /// <param name="extectedTopSequenceNumber">
        /// If this is set and the sequence number of the event stream is higher then the event is not written
        /// </param>
        /// <param name="eventVersionNumber">
        /// The version number of the event being written
        /// </param>
        Task AppendEvent(IEvent eventInstance,
            int extectedTopSequenceNumber = 0,
            int eventVersionNumber = 1);


        /// <summary>
        /// Set the context under which this event stream writer is writing events
        /// </summary>
        /// <param name="writerContext">
        /// The additional context information to add to the events when they are written
        /// </param>
        void SetContext(IWriteContext writerContext);

    }
}
