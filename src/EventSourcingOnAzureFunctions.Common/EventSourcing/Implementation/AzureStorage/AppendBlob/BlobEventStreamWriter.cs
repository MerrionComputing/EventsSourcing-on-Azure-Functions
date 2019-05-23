using EventSourcingOnAzureFunctions.Common.EventSourcing.Exceptions;
using EventSourcingOnAzureFunctions.Common.EventSourcing.Interfaces;
using Microsoft.Azure.Storage;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace EventSourcingOnAzureFunctions.Common.EventSourcing.Implementation.AzureStorage.AppendBlob
{
    public sealed class BlobEventStreamWriter
        : BlobEventStreamBase , IEventStreamWriter
    {


        /// <summary>
        /// Append the event to the end of the event stream
        /// </summary>
        /// <param name="eventInstance">
        /// The event to append to the end of the event stream
        /// </param>
        /// <param name="expectedTopSequenceNumber">
        /// if this is set to > 0 and the event stream is further on then a consistency issue has arisen and the 
        /// event should not be written but rather throw an error
        /// </param>
        /// <param name="eventVersionNumber">
        /// The version number to add to the event wrapper
        /// </param>
        /// <returns></returns>
        public async Task AppendEvent(IEvent eventInstance,
            int expectedTopSequenceNumber = 0, 
            int eventVersionNumber = 1)
        {
            if (base.EventStreamBlob != null)
            {
                int nextSequence = await base.GetSequenceNumber() + 1;

                if (expectedTopSequenceNumber > 0)
                {
                    // check against actual top sequence number
                    if ((expectedTopSequenceNumber + 1) < nextSequence )
                    {
                        throw new EventStreamWriteException(this,
                            (nextSequence - 1),
                            message: $"Out of sequence write - expected seqeunce number {expectedTopSequenceNumber }",
                            source: "Blob Event Stream Writer");
                    }
                }

                string eventName = "" ;
                if (null != eventInstance )
                {
                    eventName = EventNameAttribute.GetEventName(eventInstance.GetType());
                }

                BlobBlockJsonWrappedEvent evtToWrite = BlobBlockJsonWrappedEvent.Create(eventName,
                    nextSequence,
                    eventVersionNumber,
                    null,
                    eventInstance,
                    _writerContext);

                try
                {
                    await EventStreamBlob.AppendBlockAsync(new System.IO.MemoryStream(Encoding.UTF8.GetBytes(evtToWrite.ToJSonText())));
                }
                catch (StorageException exBlob)
                {
                    throw new EventStreamWriteException(this,
                            (nextSequence - 1),
                            message: "Failed to save an event to the event stream",
                            source: "Blob Event Stream Writer",
                            innerException: exBlob );
                }

                await IncrementSequence();

            }
            
        }

        /// <summary>
        /// Increment the sequence number of the event stream
        /// </summary>
        private async Task IncrementSequence()
        {

            if (null != EventStreamBlob)
            {
                bool exists = await EventStreamBlob.ExistsAsync();
                if (exists)
                {
                    await EventStreamBlob.FetchAttributesAsync();
                    int sequenceNumber;
                    if (int.TryParse(EventStreamBlob.Metadata[METADATA_SEQUENCE], out sequenceNumber))
                    {
                        sequenceNumber += 1;
                        EventStreamBlob.Metadata[METADATA_SEQUENCE] = $"{sequenceNumber }";
                        // and commit it back
                        await EventStreamBlob.SetMetadataAsync();
                    }
                }
            }

        }

        /// <summary>
        /// Does an event stream already exist for this Domain/Type/Instance
        /// </summary>
        /// <remarks>
        /// This can be used for e.g. checking it exists as part of a validation
        /// </remarks>
        public Task<bool> Exists()
        {
            if (base.EventStreamBlob != null)
            {
                return base.EventStreamBlob.ExistsAsync();
            }
            else
            {
                // If the blob doesn't exist then the event stream doesn't exist
                return Task.FromResult<bool>(false); 
            }
        }


        private IWriteContext _writerContext;
        public void SetContext(IWriteContext writerContext)
        {
            _writerContext = writerContext;
        }

        public BlobEventStreamWriter(IEventStreamIdentity identity,
            string connectionStringName = @"")
           : base(identity,
                 writeAccess: true,
                 connectionStringName: connectionStringName)
        {

        }


    }
}
