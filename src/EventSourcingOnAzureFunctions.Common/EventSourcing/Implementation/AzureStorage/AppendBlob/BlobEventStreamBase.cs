using EventSourcingOnAzureFunctions.Common.EventSourcing.Exceptions;
using EventSourcingOnAzureFunctions.Common.EventSourcing.Interfaces;
using Microsoft.Azure.Storage.Blob;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace EventSourcingOnAzureFunctions.Common.EventSourcing.Implementation.AzureStorage.AppendBlob
{
    public abstract class BlobEventStreamBase
        : AzureStorageEventStreamBase, 
        IEventStreamIdentity
    {

        // Named additional special attributes for the append blob 
        public const string METATDATA_DOMAIN = "DOMAIN";
        public const string METADATA_ENTITY_TYPE_NAME = "ENTITYTYPENAME";
        public const string METADATA_SEQUENCE = "SEQUENCE";
        public const string METADATA_INSTANCE_KEY = "INSTANCEKEY";
        public const string METADATA_DATE_CREATED = "DATECREATED";
        public const string METADATA_CORRELATION_ID = "CORRELATIONIDENTIFIER";

        // Named specific sub-folders
        public const string ORPHANS_FOLDER = "uncategorised";
        public const string EVENTSTREAM_FOLDER = "eventstreams";
        public const string SNAPSHOTS_FOLDER = "snapshots";

        private readonly CloudBlobContainer _blobBasePath;



        /// <summary>
        /// The container where the blob for this specific event stream can be found
        /// </summary>
        public CloudBlobContainer BlobContainer
        {
            get
            {
               return  _blobBasePath;
            }
        }

        private readonly CloudAppendBlob _blob;
        public CloudAppendBlob EventStreamBlob
        {
            get
            {
                return _blob;
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



        /// <summary>
        /// Get the number of records in this event stream
        /// </summary>
        /// <returns>
        /// The number of events appended to this event stream
        /// </returns>
        public int GetRecordCount()
        {
            if (null != EventStreamBlob )
            {
                return (EventStreamBlob.Properties.AppendBlobCommittedBlockCount.GetValueOrDefault());
            }
            else
            {
                // No blob exists - throw an exception ?
                throw new EventStreamReadException(this,
                    0,
                    "Event stream blob not initialised");
            }
        }

        /// <summary>
        /// Gets the current top sequence number of the event stream
        /// </summary>
        public async Task<int> GetSequenceNumber()
        {

            if (null != EventStreamBlob )
            {
                bool exists = await EventStreamBlob.ExistsAsync();
                if (exists)
                {
                    await EventStreamBlob.FetchAttributesAsync();
                    int sequenceNumber;
                    if (int.TryParse(EventStreamBlob.Metadata[METADATA_SEQUENCE ], out sequenceNumber  ))
                    {
                        return sequenceNumber;
                    }
                }
            }

            return 0;
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

        public string EventStreamBlobFilename
        {
            get
            {
                return GetEventStreamStorageFolderPath(EntityTypeName) + '/' + MakeValidStorageFolderName(InstanceKey) + @".events";
            }
        }

        private readonly CloudBlobClient _blobClient;


        /// <summary>
        /// Make sure the blob info attributes are up to date 
        /// </summary>
        /// <remarks>
        /// This is similar in concept to FileSystemInfo.Refresh
        /// </remarks>
        public async Task Refresh()
        {
            if (EventStreamBlob != null)
            {
                bool exists = await EventStreamBlob.ExistsAsync();
                if (exists )
                {
                    // just refresh the attributes
                    await EventStreamBlob.FetchAttributesAsync(); 
                }
                else
                {
                    await EventStreamBlob.CreateOrReplaceAsync();
                    // Set the original metadata
                    EventStreamBlob.Metadata[METATDATA_DOMAIN] = DomainName;
                    EventStreamBlob.Metadata[METADATA_ENTITY_TYPE_NAME] = EntityTypeName;
                    EventStreamBlob.Metadata[METADATA_INSTANCE_KEY] = InstanceKey;
                    EventStreamBlob.Metadata[METADATA_SEQUENCE] = @"0";
                    EventStreamBlob.Metadata[METADATA_DATE_CREATED] = DateTime.UtcNow.ToString("O");
                    // and commit it back
                    await EventStreamBlob.SetMetadataAsync(); 
                }
            }
        }


        public BlobEventStreamBase(IEventStreamIdentity identity,
            bool writeAccess = false,
            string connectionStringName = @"")
            : base(identity.DomainName , writeAccess, connectionStringName )
        {

            if (null != _storageAccount)
            {
                _blobClient = _storageAccount.CreateCloudBlobClient();
                if (null != _blobClient)
                {
                    string containerFullPath = GetEventStreamStorageFolderPath(identity.DomainName, identity.EntityTypeName);
                    _blobBasePath = _blobClient.GetContainerReference(containerFullPath);
                }
            }

            _entityTypeName = identity.EntityTypeName ;
            _instanceKey = identity.InstanceKey;

            // make the blob reference that will hold the events
            _blob = _blobBasePath.GetAppendBlobReference(EventStreamBlobFilename);

        }

        /// <summary>
        /// Turn a name into a valid folder name for azure blob storage
        /// </summary>
        /// <param name="rawName">
        /// The name of the thing we want to turn into a blob storage folder name
        /// </param>
        /// <returns>
        /// A folder name that can be used to locate this object type's event streams
        /// </returns>
        /// <remarks>
        /// Container names must start With a letter Or number, And can contain only letters, numbers, And the dash (-) character.
        /// Every dash (-) character must be immediately preceded And followed by a letter Or number; consecutive dashes are Not permitted in container names.
        /// All letters in a container name must be lowercase.
        /// Container names must be from 3 through 63 characters long.
        /// </remarks>
        public static string MakeValidStorageFolderName(string rawName)
        {

            if (string.IsNullOrWhiteSpace(rawName ) )
            {
                return ORPHANS_FOLDER;
            }


            char[] invalidCharacters = @" _!,.;':@£$%^&*()+=/\#~{}[]?<>".ToCharArray();
            string cleanName = string.Join('-', rawName.Split(invalidCharacters));

            if (cleanName.StartsWith('-')  )
            {
                cleanName = cleanName.TrimStart('-'); 
            }

            if (cleanName.EndsWith ('-'))
            {
                cleanName = cleanName.TrimEnd('-');
            }

            if (cleanName.Length < 3)
            {
                cleanName += "-abc";
            }

            cleanName = cleanName.Replace("--", "-"); 

            if (cleanName.Length > 63)
            {
                string uniqueId = cleanName.GetHashCode().ToString();
                cleanName = cleanName.Substring(0, 63 - uniqueId.Length) + uniqueId;
            }

            return cleanName.ToLowerInvariant();
            
        }

        public static string GetEventStreamStorageFolderPath(string domainName, string entityType)
        {

            string domainPath = "";

            foreach (string domainSection in domainName.Split("."))
            {
                if (!string.IsNullOrWhiteSpace(domainSection))
                {
                    if (!string.IsNullOrWhiteSpace(domainPath))
                    {
                        domainPath += "/";
                    }
                    domainPath += MakeValidStorageFolderName(domainSection);
                }
            }

            return domainPath + '/' + MakeValidStorageFolderName(entityType);
        }

        public static string GetEventStreamStorageFolderPath( string entityType)
        {
            return  EVENTSTREAM_FOLDER;
        }
    }
}
