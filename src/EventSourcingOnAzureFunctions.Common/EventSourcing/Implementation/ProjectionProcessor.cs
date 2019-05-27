using EventSourcingOnAzureFunctions.Common.EventSourcing.Implementation.AzureStorage.AppendBlob;
using EventSourcingOnAzureFunctions.Common.EventSourcing.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace EventSourcingOnAzureFunctions.Common.EventSourcing.Implementation
{
    public sealed class ProjectionProcessor
        : IProjectionProcessor
    {

        private readonly IEventStreamReader eventStreamReader = null;

        public async Task<TProjection> Process<TProjection>() where TProjection : IProjection, new()
        {
            TProjection ret = new TProjection();

            if (null != eventStreamReader)
            {
                foreach (IEventContext wrappedEvent in await eventStreamReader.GetEventsWithContext())
                {
                    // TODO: get as-of date from the event
                    ret.OnEventRead(wrappedEvent.SequenceNumber, null);

                    if (ret.HandlesEventType(wrappedEvent.EventInstance.EventTypeName  ) )
                    {
                        ret.HandleEvent(wrappedEvent.EventInstance.EventTypeName, wrappedEvent.EventInstance.EventPayload);
                    }

                    // mark the event as handled
                    ret.MarkEventHandled(wrappedEvent.SequenceNumber);
                }
            }

            return  ret;
        }

        /// <summary>
        /// Does the underlying event stream over which this projection should run exists yet?
        /// </summary>
        public async Task<bool> Exists()
        {
            if (null != eventStreamReader)
            {
                return await  eventStreamReader.Exists();
            }
            return false;
        }

        public ProjectionProcessor(BlobEventStreamReader blobEventStreamReader)
        {
            // Initialise the reader to use to read the events to be processed
            this.eventStreamReader = blobEventStreamReader;
        }
    }
}
