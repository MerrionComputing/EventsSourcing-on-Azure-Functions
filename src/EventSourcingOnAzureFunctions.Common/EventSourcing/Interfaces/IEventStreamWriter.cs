using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using static EventSourcingOnAzureFunctions.Common.EventSourcing.Implementation.EventStreamBase;

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
        /// <param name="expectedTopSequenceNumber">
        /// If this is set and the sequence number of the event stream is higher then the event is not written
        /// </param>
        /// <param name="eventVersionNumber">
        /// The version number of the event being written
        /// </param>
        Task<IAppendResult> AppendEvent(IEvent eventInstance,
            int expectedTopSequenceNumber = 0,
            int eventVersionNumber = 1,
            EventStreamExistenceConstraint streamConstraint = EventStreamExistenceConstraint.Loose);


        /// <summary>
        /// Set the context under which this event stream writer is writing events
        /// </summary>
        /// <param name="writerContext">
        /// The additional context information to add to the events when they are written
        /// </param>
        void SetContext(IWriteContext writerContext);

        /// <summary>
        /// Returns true if the underlying event stream exists
        /// </summary>
        /// <returns></returns>
        Task<bool> Exists();


        /// <summary>
        /// Delete the underlying event stream of this event stream identity
        /// </summary>
        void DeleteStream();

        /// <summary>
        /// Write an index card record to be used to be used rapidly to look up the
        /// full set of event streams of this type
        /// </summary>
        Task WriteIndex();
    }
}
