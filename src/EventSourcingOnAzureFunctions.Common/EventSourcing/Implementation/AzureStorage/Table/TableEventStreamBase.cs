using EventSourcingOnAzureFunctions.Common.EventSourcing.Exceptions;
using EventSourcingOnAzureFunctions.Common.EventSourcing.Interfaces;
using Microsoft.Azure.CosmosDB.Table;
using System;
using System.Reflection;
using System.Threading.Tasks;

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


        public const string RECORDID_SEQUENCE = "0000000000"; // To fint maxint32 = 2147483647


        public const int MAX_FREE_DATA_FIELDS = 248;
        public const string ORPHANS_TABLE = "Uncategorised";

        public const int MAX_BATCH_SIZE = 100;

        private readonly CloudTableClient _cloudTableClient;
        
        public CloudTable Table
        {
            get
            {
                if (null != _cloudTableClient )
                {
                    return _cloudTableClient.GetTableReference(this.TableName); 
                }
                else
                {
                    return null;
                }
            }
        }

        private readonly string _domainName;
        /// <summary>
        /// The domain in which this event stream is set
        /// </summary>
        public string DomainName
        {
            get
            {
                return _domainName;
            }
        }

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

        public string TableName
        {
            get
            {
                return MakeValidStorageTableName(_entityTypeName + @"00" + base.DomainName);
            }
        }

        public Task<bool> Exists()
        {
            if (Table != null)
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



        public TableEventStreamBase(IEventStreamIdentity identity,
            bool writeAccess = false,
            string connectionStringName = @"")
            : base(identity.DomainName, writeAccess, connectionStringName)
        {

            _domainName = identity.DomainName;
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


        /// <summary>
        /// Return the sequence number as a string for storing in the RowKey field of an Azure table
        /// </summary>
        /// <param name="startingSequence">
        /// The given sequence number
        /// </param>
        /// <returns></returns>
        public static string SequenceNumberAsString(int startingSequence)
        {
            return startingSequence.ToString(RECORDID_SEQUENCE);
        }

        /// <summary>
        /// Generate a query to get the event rows for an individual event stream between two sequence numbers (inclusive)
        /// </summary>
        /// <param name="identity">
        /// The unique identifier of the event stream to read
        /// </param>
        /// <param name="startingSequence">
        /// The starting sequence to read from
        /// </param>
        /// <param name="upToSequence">
        /// (Optional) The end sequence to read up to (inclusive)
        /// </param>
        public static TableQuery<DynamicTableEntity> ProjectionQuery(IEventStreamIdentity identity,
            int startingSequence = 1,
            int upToSequence = 0)
        {

            if (upToSequence > 0)
            {
                if (startingSequence >= upToSequence)
                {
                    throw new EventStreamReadException(identity,
                        startingSequence,
                        $"Requested end sequence {upToSequence} is less than or equal to start sequence {startingSequence}");
                }
            }

            TableQuery<DynamicTableEntity> ret = InstanceQuery(identity).Where(TableQuery.GenerateFilterCondition("RowKey",
                    QueryComparisons.GreaterThanOrEqual, SequenceNumberAsString(startingSequence)));

            if (upToSequence > 0)
            {
                ret = ret.Where(TableQuery.GenerateFilterCondition("RowKey",
                    QueryComparisons.LessThanOrEqual, SequenceNumberAsString(upToSequence)));
            }

            return ret;
        }



        /// <summary>
        /// Generate a query definition for the given entity instance
        /// </summary>
        /// <param name="identity">
        /// The domain/entity type/unique identifier insatnec of the event stream to get
        /// </param>
        /// <returns></returns>
        private static TableQuery<DynamicTableEntity> InstanceQuery(IEventStreamIdentity identity)
        {
            return new TableQuery<DynamicTableEntity>()
                .Where(TableQuery.GenerateFilterCondition("PartitionKey",
                    QueryComparisons.Equal, identity.InstanceKey));
        }

        public static bool IsPropertyEmpty(PropertyInfo pi, object eventInstance)
        {
            if (null == pi.GetValue(eventInstance))
            {
                return true;
            }

            // special case - dates before 1601
            if (pi.PropertyType == typeof(DateTime ))
            {
                DateTime val = (DateTime)pi.GetValue(eventInstance, null);
                if (val.Year < 1601)
                {
                    return true;
                }
            }
            if (pi.PropertyType == typeof(DateTimeOffset))
            {
                DateTimeOffset val = (DateTimeOffset)pi.GetValue(eventInstance, null);
                if (val.Year < 1601)
                {
                    return true;
                }
            }

            return false;
        }


        /// <summary>
        /// Returns true if a property belongs to the event context rather thab the event data
        /// </summary>
        /// <param name="propertyName">
        /// The name of the property from the table
        /// </param>
        public static bool IsContextProperty(string propertyName)
        {

            if (propertyName.Equals(FIELDNAME_EVENTTYPE, StringComparison.OrdinalIgnoreCase   ))
            {
                return true;
            }

            if (propertyName.Equals(FIELDNAME_CORRELATION_IDENTIFIER, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            if (propertyName.Equals(FIELDNAME_COMMENTS, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            if (propertyName.Equals(FIELDNAME_SOURCE, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            if (propertyName.Equals(FIELDNAME_VERSION, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            if (propertyName.Equals(FIELDNAME_WHO, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            return false;
        }

    }
}
