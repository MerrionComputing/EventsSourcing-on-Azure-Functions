using EventSourcingOnAzureFunctions.Common.EventSourcing.Interfaces;
using Microsoft.Azure.Cosmos.Table;
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


        /// <summary>
        /// Policy to allow read-access to the tables
        /// </summary>
        public SharedAccessAccountPolicy DefaultSharedAccessAccountPolicy
        {
            get
            {
                // Make a standard shared access policy to use 
                return new SharedAccessAccountPolicy()
                {
                    Permissions =  SharedAccessAccountPermissions.Read
                    | SharedAccessAccountPermissions.List
                    ,
                    ResourceTypes = SharedAccessAccountResourceTypes.Object,
                    Services = SharedAccessAccountServices.Table,
                    Protocols = SharedAccessProtocol.HttpsOnly
                };
            }
        }

        public SharedAccessTablePolicy DefaultSharedAccessTablePolicy
        {
            get
            {
                return new SharedAccessTablePolicy()
                {
                    Permissions = SharedAccessTablePermissions.Query 
                };
            }
        }

        public TableEventStreamReader(IEventStreamIdentity identity,
            string connectionStringName = @"")
            : base(identity, false, connectionStringName )
        {

        }
    }
}
