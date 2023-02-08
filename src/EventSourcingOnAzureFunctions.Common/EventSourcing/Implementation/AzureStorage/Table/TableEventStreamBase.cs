using EventSourcingOnAzureFunctions.Common.EventSourcing.Exceptions;
using EventSourcingOnAzureFunctions.Common.EventSourcing.Interfaces;
using Azure.Data.Tables;
using Microsoft.Extensions.Configuration;
using System;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;

namespace EventSourcingOnAzureFunctions.Common.EventSourcing.Implementation.AzureStorage.Table
{
    public abstract class TableEventStreamBase
        : AzureStorageEventStreamBase, 
        IEventStreamIdentity
    {

        #region Field names
        public const string FIELDNAME_EVENTTYPE = "EventType";
        public const string FIELDNAME_VERSION = "Version";
        public const string FIELDNAME_COMMENTS = "ContextCommentary";
        public const string FIELDNAME_WHO = "ContextWho";
        public const string FIELDNAME_SOURCE = "ContextSource";
        public const string FIELDNAME_SCHEMA = "ContextSchema";
        public const string FIELDNAME_CORRELATION_IDENTIFIER = "CorrelationIdentifier";
        public const string FIELDNAME_CAUSATION_IDENTIFIER = "CausationIdentifier";
        #endregion


        public const string RECORDID_SEQUENCE = "0000000000"; // To fit maxint32 = 2147483647


        public const int MAX_FREE_DATA_FIELDS = 240;
        public const string ORPHANS_TABLE = "Uncategorised";

        public const int MAX_BATCH_SIZE = 100;

        private readonly TableClient _cloudTableClient;

        public TableClient Table
        {
            get
            {
                if (null != _cloudTableClient )
                {
                    return _cloudTableClient;
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
                return MakeValidStorageTableName(_entityTypeName + @"00" + DomainName);
            }
        }

        public async Task<bool> Exists()
        {
            if (Table != null)
            {
                TableEntityIndexCardRecord ret = null;
                try
                {
                     ret = await Table.GetEntityAsync<TableEntityIndexCardRecord>(TableEntityIndexCardRecord.INDEX_CARD_PARTITION, this.InstanceKey);
                }
                catch (Azure.RequestFailedException ex)
                {
                    // Need to create a new stream footer if not found...but rethrow any other error
                    if (ex.Status != 404)
                    {
                        throw;
                    }
                }

                if (null != ret)
                { 
                    return true;
                }
                return await Task.FromResult<bool>(false);
            }
            else
            {
                // If the table doesn't exist then the instance cannot possibly exist
                return await Task.FromResult<bool>(false);
            }

        }

        /// <summary>
        /// Does a stream already exist for this event stream identity
        /// </summary>
        /// <remarks>
        /// We use the existence of the stream footer record as proof of stream existence
        /// </remarks>
        protected async internal Task<bool> StreamAlreadyExists()
        {
            TableEntityKeyRecord streamFooter = null;


            await Table.CreateIfNotExistsAsync();

            try
            {
                streamFooter = await Table.GetEntityAsync<TableEntityKeyRecord>(this.InstanceKey, SequenceNumberAsString(0));
            }
            catch (Azure.RequestFailedException ex)
            {
                // Allow if stream footer not found...but rethrow any other error
                if (ex.Status != 404)
                {
                    throw;
                }
            }


            if (null != streamFooter)
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// The name of the default connection string to use for the domain
        /// </summary>
        protected internal string StorageConnectionStringSettingName
        {
            get
            {
                char[] invalidCharacters = @" _!,.;':@£$%^&*()+=/\#~{}[]?<>".ToCharArray();
                return string.Join("", DomainName.Split(invalidCharacters)).Trim() + "TableStorageConnectionString";
            }
        }

        public TableEventStreamBase(IEventStreamIdentity identity,
            bool writeAccess = false,
            string connectionStringName = @"")
            : base(identity.DomainName, writeAccess,connectionStringName )
        {

            _domainName = identity.DomainName;
            _entityTypeName = identity.EntityTypeName;
            _instanceKey = identity.InstanceKey;


            // Set the connection string to use
            if (string.IsNullOrWhiteSpace(connectionStringName))
            {
                connectionStringName = StorageConnectionStringSettingName;
            }

            // Create a connection to the cloud storage account to use
            ConfigurationBuilder builder = new ConfigurationBuilder();
            builder.SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", true)
                .AddJsonFile("local.settings.json", true)
                .AddJsonFile("config.local.json", true)
                .AddJsonFile("config.json", true)
                .AddJsonFile("connectionstrings.json", true)
                .AddEnvironmentVariables();

            IConfigurationRoot config = builder.Build();

            if (null != config)
            {
                if (!string.IsNullOrWhiteSpace(connectionStringName))
                {
                    string connectionString = config.GetConnectionString(connectionStringName);
                    if (string.IsNullOrWhiteSpace(connectionString ) )
                    {
                        throw new NullReferenceException($"No connection string configured for {connectionStringName}");  
                    }

                    if (_cloudTableClient == null)
                    {
                        _cloudTableClient = new TableClient(connectionString, TableName);
                    }

                }
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

        public static int SequenceNumberFromString(string sequenceNumberAsString)
        {
            if (string.IsNullOrWhiteSpace(sequenceNumberAsString ) )
            {
                return 0;
            }
            else
            {
                int seqRet;
                if (int.TryParse(sequenceNumberAsString.TrimStart('0') , out seqRet ) )
                {
                    return seqRet;
                }
            }
            return 0;
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
        public static string ProjectionQuery(IEventStreamIdentity identity,
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

            System.FormattableString   filter = $"PartitionKey eq {identity.InstanceKey} and RowKey ge {SequenceNumberAsString(startingSequence)} { ((upToSequence > 0) ? " and RowKey le {SequenceNumberAsString(upToSequence)} }" : "") }";

            
            return TableClient.CreateQueryFilter(filter) ;
        }



        /// <summary>
        /// Generate a query definition for the given entity instance
        /// </summary>
        /// <param name="identity">
        /// The domain/entity type/unique identifier insatnec of the event stream to get
        /// </param>
        /// <returns></returns>
        private static string InstanceQuery(IEventStreamIdentity identity)
        {
            return TableClient.CreateQueryFilter($"PartitionKey eq {identity.InstanceKey} ");
        }

        /// <summary>
        /// Is the property empty so not to be persisted to the backing store
        /// </summary>
        public static bool IsPropertyEmpty(PropertyInfo pi, object eventInstance)
        {
            if (null == pi.GetValue(eventInstance))
            {
                return true;
            }

            return IsPropertyValueEmpty(pi, pi.GetValue(eventInstance, null));
        }

        public static bool IsPropertyValueEmpty(PropertyInfo pi, object propertyValue)
        {
            if (null == propertyValue)
            {
                return true;
            }

            // special case - dates before 1601
            if (pi.PropertyType == typeof(DateTime))
            {
                if (propertyValue.GetType() == typeof(DateTime))
                {
                    DateTime val = (DateTime)propertyValue;
                    if (val.Year < 1601)
                    {
                        return true;
                    }
                }
            }
            if (pi.PropertyType == typeof(DateTimeOffset))
            {
                DateTimeOffset val = (DateTimeOffset)propertyValue;
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

            if (propertyName.Equals(FIELDNAME_CAUSATION_IDENTIFIER , StringComparison.OrdinalIgnoreCase))
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

            if (propertyName.Equals(FIELDNAME_SCHEMA, StringComparison.OrdinalIgnoreCase )  )
            {
                return true;
            }

            return false;
        }


        public static object GetEntityPropertyValue(PropertyInfo pi, object propertyAsObject)
        {
            if (pi.PropertyType == typeof(Decimal))
            {
                double dblValue = (double)propertyAsObject;
                return new decimal(dblValue);
            }
            if (pi.PropertyType.IsEnum )
            {
                return propertyAsObject.ToString();
            }

            return propertyAsObject;
        }

    }
}
