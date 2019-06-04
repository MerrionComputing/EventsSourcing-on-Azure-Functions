using EventSourcingOnAzureFunctions.Common.EventSourcing.Interfaces;
using Microsoft.Azure.CosmosDB.Table;
using System;

namespace EventSourcingOnAzureFunctions.Common.EventSourcing.Implementation.AzureStorage.Table
{
    public abstract class TableEventStreamBase
        : AzureStorageEventStreamBase, IEventStreamIdentity
    {

        #region Field names
        public const string FIELDNAME_EVENTTYPE = "EventType";
        public const string FIELDNAME_VERSION = "Version";
        public const string FIELDNAME_COMMENTS = "Commentary";
        public const string FIELDNAME_WHO = "Who";
        public const string FIELDNAME_SOURCE = "Source";
        public const string FIELDNAME_CORRELATION_IDENTIFIER = "CorrelationIdentifier";
        #endregion


        public const int MAX_FREE_DATA_FIELDS = 248;
        public const string ORPHANS_TABLE = "Uncategorised";

        private readonly CloudTableClient _cloudTableClient;

        private readonly string _entityTypeName;
        /// <summary>
        /// The type of entity for which this event stream pertains
        /// </summary>
        public string EntityTypeName
        {
            get
            {
                return _entityTypeName;
            }
        }

        private readonly string _instanceKey;
        /// <summary>
        /// The specific uniquely identitified instance of the entity to which this event stream pertains
        /// </summary>
        public string InstanceKey
        {
            get
            {
                return _instanceKey;
            }
        }



        public TableEventStreamBase(IEventStreamIdentity identity,
            bool writeAccess = false,
            string connectionStringName = @"")
            : base(identity.DomainName, writeAccess, connectionStringName)
        {

            _entityTypeName = identity.EntityTypeName;
            _instanceKey = identity.InstanceKey;

            if (base._storageAccount != null)
            {
                _cloudTableClient = base._storageAccount.CreateCloudTableClient();
            }


        }

        /// <summary>
        /// Make a valid Azure table name for the raw name of the entity type passed in
        /// </summary>
        /// <param name="rawName">
        /// The raw name of the entity type passed in that may be an illegal table name
        /// </param>
        public static string MakeValidStorageTableName(string rawName)
        {
            if (string.IsNullOrWhiteSpace(rawName))
            {
                //Don't allow empty table names - assign an orphan table for it
                return ORPHANS_TABLE;
            }

            char[] invalidCharacters = @" !,.;':@£$%^&*()-+=/\#~{}[]?<>_".ToCharArray();
            string cleanName = string.Join("", rawName.Split(invalidCharacters));
            if (cleanName.Length < 3 )
            {
                cleanName += @"DATA";
            }

            if (cleanName.Length > 63)
            {
                int uniqueness = Math.Abs(cleanName.GetHashCode());
                string uniqueId =  uniqueness.ToString();
                cleanName = cleanName.Substring(0, 63 - uniqueId.Length) + uniqueId;
            }

            return cleanName;
        }
    }
}
