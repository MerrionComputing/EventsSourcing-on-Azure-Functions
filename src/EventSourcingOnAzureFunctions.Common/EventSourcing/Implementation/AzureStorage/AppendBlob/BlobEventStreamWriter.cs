using EventSourcingOnAzureFunctions.Common.EventSourcing.Exceptions;
using EventSourcingOnAzureFunctions.Common.EventSourcing.Interfaces;
using Microsoft.Azure.Storage;
using System;
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
        /// <param name="streamConstraint">
        /// An additional constrain that must be satisfied by the event stream in order to persist the event
        /// </param>
        /// <returns></returns>
        public async Task<IAppendResult> AppendEvent(IEvent eventInstance,
            int expectedTopSequenceNumber = 0, 
            int eventVersionNumber = 1,
            EventStreamExistenceConstraint streamConstraint = EventStreamExistenceConstraint.Loose )
        {
            if (base.EventStreamBlob != null)
            {
            
                // acquire a lease for the blob..
                string writeStreamLeaseId = null;
                if (await Exists())
                {
                     writeStreamLeaseId = await base.EventStreamBlob.AcquireLeaseAsync(TimeSpan.FromSeconds(15));
                }

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

                // create an access condition
                AccessCondition condition = AccessCondition.GenerateEmptyCondition();
                if (streamConstraint== EventStreamExistenceConstraint.MustBeNew )
                {
                    condition = AccessCondition.GenerateIfNotExistsCondition();
                }
                if (streamConstraint== EventStreamExistenceConstraint.MustExist )
                {
                    condition = AccessCondition.GenerateIfExistsCondition(); 
                }
                if (!string.IsNullOrWhiteSpace(writeStreamLeaseId))
                {
                    condition.LeaseId = writeStreamLeaseId;
                }

                // default the writer context if it is not already set
                if (null == _writerContext)
                {
                    _writerContext = WriteContext.DefaultWriterContext();
                }

                BlobBlockJsonWrappedEvent evtToWrite = BlobBlockJsonWrappedEvent.Create(eventName,
                    nextSequence,
                    eventVersionNumber,
                    null,
                    eventInstance,
                    _writerContext);

                try
                {
                    // Create it if it doesn't exist and initialsie the metadata
                    await base.Refresh();


                    Microsoft.Azure.Storage.OperationContext context = new Microsoft.Azure.Storage.OperationContext()
                    {  };

                    await EventStreamBlob.AppendBlockAsync(new System.IO.MemoryStream(Encoding.UTF8.GetBytes(evtToWrite.ToJSonText())),
                        "", 
                        condition,
                        null , // use the default blob request options
                        context 
                        );
                }
                catch (Microsoft.Azure.Storage.StorageException exBlob)
                {
                    throw new EventStreamWriteException(this,
                            (nextSequence - 1),
                            message: "Failed to save an event to the event stream",
                            source: "Blob Event Stream Writer",
                            innerException: exBlob );
                }

                await IncrementSequence(writeStreamLeaseId);

                if (!string.IsNullOrWhiteSpace(writeStreamLeaseId))
                {
                    // and release the lease
                    await base.EventStreamBlob.ReleaseLeaseAsync(condition);
                }

                int sequence = await base.GetSequenceNumber();

                return new AppendResult((sequence == 0), sequence);
            }
            else
            {
                return null;
            }

            
        }



        /// <summary>
        /// Increment the sequence number of the event stream
        /// </summary>
        private async Task IncrementSequence(string writeStreamLeaseId = "")
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
                        AccessCondition condition = AccessCondition.GenerateEmptyCondition();
                        if (!string.IsNullOrWhiteSpace(writeStreamLeaseId))
                        {
                            condition.LeaseId = writeStreamLeaseId;
                        }
                        await EventStreamBlob.SetMetadataAsync(condition, null, new Microsoft.Azure.Storage.OperationContext() );

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

        /// <summary>
        /// Delete the blob file containing the event stream
        /// </summary>
        public async Task DeleteStream()
        {
            if (null != EventStreamBlob)
            {
                AccessCondition condition = AccessCondition.GenerateEmptyCondition();
                await EventStreamBlob.DeleteAsync(Microsoft.Azure.Storage.Blob.DeleteSnapshotsOption.IncludeSnapshots, 
                    condition , 
                    null,
                    new Microsoft.Azure.Storage.OperationContext());
            }
        }


        public Task WriteIndex()
        {
            // Currently does not do anything, as the file system query is as fast as 
            // it can be
            return Task.CompletedTask;
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
