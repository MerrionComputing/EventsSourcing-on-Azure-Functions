using EventSourcingOnAzureFunctions.Common.EventSourcing.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace EventSourcingOnAzureFunctions.Common.EventSourcing.Implementation.AzureStorage.Table
{
    public sealed class TableEventStreamReader
        : TableEventStreamBase, IEventStreamReader
    {


        public Task<IEnumerable<IEvent>> GetAllEvents()
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<IEvent>> GetEvents(int StartingSequenceNumber = 0, 
            DateTime? effectiveDateTime = null)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<IEventContext>> GetEventsWithContext(int StartingSequenceNumber = 0, 
            DateTime? effectiveDateTime = null)
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
