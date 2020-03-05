using EventSourcingOnAzureFunctions.Common.EventSourcing.Interfaces;
using Microsoft.Azure.Cosmos.Table;
using Microsoft.Extensions.Configuration;
using System.IO;

namespace EventSourcingOnAzureFunctions.Common.EventSourcing.Implementation.AzureStorage.Table
{
    /// <summary>
    /// Base class for functionality to persist classification snapshots
    /// to an Azure table
    /// </summary>
    public abstract class TableClassificationSnapshotBase
        : IEventStreamIdentity
    {

        protected internal CloudStorageAccount _storageAccount;
        private readonly CloudTableClient _cloudTableClient;

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

        private readonly string _classificationName;
        public string ClassificationName
        {
            get
            {
                return _classificationName;
            }
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

        public string TableName
        {
            get
            {
                return TableEventStreamBase.MakeValidStorageTableName(EntityTypeName + @"99" + DomainName + "99" + ClassificationName );
            }
        }

        public CloudTable Table
        {
            get
            {
                if (null != _cloudTableClient)
                {
                    CloudTable ret = _cloudTableClient.GetTableReference(this.TableName);
                    return ret;
                }
                else
                {
                    return null;
                }
            }
        }

        public TableClassificationSnapshotBase(IEventStreamIdentity identity,
            string classificationName,
            bool writeAccess = false,
            string connectionStringName = @"")
        {

            _domainName = identity.DomainName;
            _entityTypeName = identity.EntityTypeName;
            _instanceKey = identity.InstanceKey;
            _classificationName = classificationName;

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
                    _storageAccount = CloudStorageAccount.Parse(config.GetConnectionString(connectionStringName));
                }
            }

            if (_storageAccount != null)
            {
                _cloudTableClient = _storageAccount.CreateCloudTableClient();

                if (null != _cloudTableClient)
                {
                    if (!Table.Exists())
                    {
                        Table.Create();
                    }
                }
            }

        }
    }
}
