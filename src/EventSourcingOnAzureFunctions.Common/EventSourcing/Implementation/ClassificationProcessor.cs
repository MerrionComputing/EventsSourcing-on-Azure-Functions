using EventSourcingOnAzureFunctions.Common.EventSourcing.Implementation.AzureStorage.AppendBlob;
using EventSourcingOnAzureFunctions.Common.EventSourcing.Implementation.AzureStorage.Table;
using EventSourcingOnAzureFunctions.Common.EventSourcing.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EventSourcingOnAzureFunctions.Common.EventSourcing.Implementation
{
    public sealed class ClassificationProcessor
        : IClassificationProcessor
    {

        private readonly IEventStreamReader eventStreamReader = null;

        public async Task<ClassificationResponse> Classify<TClassification>(DateTime? asOfDate = null) where TClassification : IClassification, new()
        {
            TClassification classificationToRun = new TClassification();
            ClassificationResponse.ClassificationResults ret = ClassificationResponse.ClassificationResults.Unchanged;

            if (null != eventStreamReader)
            {
                foreach (IEventContext wrappedEvent in await eventStreamReader.GetEventsWithContext(effectiveDateTime: asOfDate))
                {

                    classificationToRun.OnEventRead(wrappedEvent.SequenceNumber, null);


                    if (classificationToRun.HandlesEventType(wrappedEvent.EventInstance.EventTypeName))
                    {
                        var stepResult = classificationToRun.HandleEvent(wrappedEvent.EventInstance.EventTypeName, wrappedEvent.EventInstance.EventPayload);
                        if (stepResult != ClassificationResponse.ClassificationResults.Unchanged )
                        {
                            // The classification state changed so store it as the current result
                            ret = stepResult;
                        }
                    }

                    // mark the event as handled
                    classificationToRun.MarkEventHandled(wrappedEvent.SequenceNumber);
                }
            }

            return new ClassificationResponse(ret, classificationToRun.CurrentSequenceNumber ) ;
        }

        /// <summary>
        /// Does the underlying event stream over which this classification should run exist yet?
        /// </summary>
        public async Task<bool> Exists()
        {
            if (null != eventStreamReader)
            {
                return await eventStreamReader.Exists();
            }
            return false;
        }


        public async Task<IEnumerable<string>> GetAllInstanceKeys(DateTime? asOfDate)
        {
            if (null != eventStreamReader)
            {
                return await eventStreamReader.GetAllInstanceKeys(asOfDate);
            }
            else
            {
                return Enumerable.Empty<string>();
            }
        }

        public ClassificationProcessor(BlobEventStreamReader blobEventStreamReader)
        {
            // Initialise the reader to use to read the events to be processed
            this.eventStreamReader = blobEventStreamReader;
        }

        public ClassificationProcessor(TableEventStreamReader tableEventStreamReader)
        {
            // Initialise the reader to use to read the events to be processed
            this.eventStreamReader = tableEventStreamReader;
        }
    }
}
