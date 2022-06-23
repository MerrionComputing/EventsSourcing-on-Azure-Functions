using EventSourcingOnAzureFunctions.Common.EventSourcing.Exceptions;
using EventSourcingOnAzureFunctions.Common.EventSourcing.Interfaces;
using Azure.Data.Tables;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EventSourcingOnAzureFunctions.Common.EventSourcing.Implementation.AzureStorage.AppendBlob
{

    public sealed class BlobEventStreamReader
        : BlobEventStreamBase, IEventStreamReader
    {



        public async Task<IEnumerable<IEvent>> GetAllEvents()
        {
            return await GetEvents(StartingSequenceNumber: 0, effectiveDateTime: null);
        }

        public async Task<IEnumerable<IEvent>> GetEvents(int StartingSequenceNumber = 0, DateTime? effectiveDateTime = null)
        {

            if (null == EventStreamBlob)
            {
                using (System.IO.Stream rawStream = await GetUnderlyingStream())
                {
                    if (!(rawStream.Position >= rawStream.Length))
                    {
                        List<IEvent> ret = new List<IEvent>();
                        foreach (BlobBlockJsonWrappedEvent record in BlobBlockJsonWrappedEvent.FromBinaryStream(rawStream))
                        {
                            if (null != record)
                            {
                                if (record.SequenceNumber >= StartingSequenceNumber)
                                {
                                    if ((!effectiveDateTime.HasValue) || (record.WriteTime <= effectiveDateTime.Value ))
                                    {
                                        ret.Add(record.EventInstance);
                                    }
                                }
                            }
                        }

                        return ret;
                    }
                }
            }

            return Enumerable.Empty<IEvent>();

        }

        public async Task<IEnumerable<IEventContext>> GetEventsWithContext(int StartingSequenceNumber = 0, DateTime? effectiveDateTime = null)
        {

            if (null != EventStreamBlob)
            {
                if (await EventStreamBlob.ExistsAsync())
                {
                    using (System.IO.Stream rawStream = await GetUnderlyingStream())
                    {
                        if (!(rawStream.Position >= rawStream.Length))
                        {
                            List<IEventContext> ret = new List<IEventContext>();
                            foreach (BlobBlockJsonWrappedEvent record in BlobBlockJsonWrappedEvent.FromBinaryStream(rawStream))
                            {
                                if (null != record)
                                {
                                    if (record.SequenceNumber >= StartingSequenceNumber)
                                    {
                                        if ((!effectiveDateTime.HasValue) || (record.WriteTime <= effectiveDateTime.Value))
                                        {
                                            ret.Add(record);
                                        }
                                    }
                                }
                            }

                            return ret;
                        }
                    }
                }
            }

            return Enumerable.Empty<IEventContext>();

        }


        private async Task<System.IO.Stream> GetUnderlyingStream()
        {
            if (null != EventStreamBlob)
            {
                System.IO.MemoryStream targetStream = new System.IO.MemoryStream();
                try
                {
                    await EventStreamBlob.DownloadToStreamAsync(targetStream);
                }
                catch (Exception exBlob)
                {
                    throw new EventStreamReadException(this, 0, "Unable to access the underlying event stream",
                        innerException: exBlob,
                        source: nameof(BlobEventStreamReader));
                }
                targetStream.Seek(0, System.IO.SeekOrigin.Begin);
                return targetStream;
            }

            return null;
        }


        public BlobEventStreamReader(IEventStreamIdentity identity,
            string connectionStringName = @"")
            : base(identity,
                  writeAccess: false,
                  connectionStringName: connectionStringName)
        {

        }

        /// <summary>
        /// Creates an azure blob storage based event stream reader for the given aggregate
        /// </summary>
        /// <param name="identity">
        /// The unique identifier of the event stream to read
        /// </param>
        /// <param name="connectionStringName">
        /// Th ename of the connection string to use to do the reading
        /// </param>
        public static BlobEventStreamReader Create(IEventStreamIdentity identity,
            string connectionStringName = @"")
        {
            return new BlobEventStreamReader(identity, connectionStringName);
        }


        public static ProjectionProcessor CreateProjectionProcessor(IEventStreamIdentity identity,
            string connectionStringName = @"")
        {
            return new ProjectionProcessor(Create(identity, connectionStringName));
        }

        public static ClassificationProcessor CreateClassificationProcessor(IEventStreamIdentity identity,
           string connectionStringName = @"")
        {
            return new ClassificationProcessor(Create(identity, connectionStringName));
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

        /// <summary>
        /// Get all of the unique instances of this domain/entity type
        /// </summary>
        /// <param name="asOfDate">
        /// (Optional) The date as of which to get all the instance keys
        /// </param>
        /// <remarks>
        /// This is to allow for set-based functionality
        /// </remarks>
        public Task<IEnumerable<string>> GetAllInstanceKeys(DateTime? asOfDate)
        {
            throw new NotImplementedException();
        }
    }
}
