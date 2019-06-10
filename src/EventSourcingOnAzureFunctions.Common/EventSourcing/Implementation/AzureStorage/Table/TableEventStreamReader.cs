using EventSourcingOnAzureFunctions.Common.EventSourcing.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace EventSourcingOnAzureFunctions.Common.EventSourcing.Implementation.AzureStorage.Table
{
    public class TableEventStreamReader
        : TableEventStreamBase, IEventStreamReader
    {
        public Task<bool> Exists()
        {
            if (base.Table  != null)
            {
                // TODO: Get zero-eth entry...
                throw new NotImplementedException();
            }
            else
            {
                // If the table doesn't exist then the event cannot possibly exist
                return Task.FromResult<bool>(false);
            }
        }

        public Task<IEnumerable<IEvent>> GetAllEvents()
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<IEvent>> GetEvents(int StartingSequenceNumber = 0, DateTime? effectiveDateTime = null)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<IEventContext>> GetEventsWithContext(int StartingSequenceNumber = 0, DateTime? effectiveDateTime = null)
        {
            throw new NotImplementedException();
        }


        public TableEventStreamReader(IEventStreamIdentity identity,
            string connectionStringName = @"")
            : base(identity, false, connectionStringName )
        {

        }
    }
}
