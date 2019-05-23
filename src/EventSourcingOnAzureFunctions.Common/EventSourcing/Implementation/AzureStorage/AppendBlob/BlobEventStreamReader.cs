using EventSourcingOnAzureFunctions.Common.EventSourcing.Interfaces;
using System;
using System.Collections.Generic;
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
            throw new NotImplementedException();
        }

        public async Task<IEnumerable<IEventContext>> GetEventsWithContext(int StartingSequenceNumber = 0, DateTime? effectiveDateTime = null)
        {
            throw new NotImplementedException();
        }

        public BlobEventStreamReader(IEventStreamIdentity identity,
            string connectionStringName = @"")
            : base(identity,
                  writeAccess:false ,
                  connectionStringName: connectionStringName )
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
    }
}
