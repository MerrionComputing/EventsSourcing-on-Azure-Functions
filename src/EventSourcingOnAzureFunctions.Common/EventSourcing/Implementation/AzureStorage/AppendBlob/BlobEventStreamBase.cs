using EventSourcingOnAzureFunctions.Common.EventSourcing.Interfaces;
using Microsoft.Azure.Storage.Blob;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace EventSourcingOnAzureFunctions.Common.EventSourcing.Implementation.AzureStorage.AppendBlob
{
    public class BlobEventStreamBase
        : AzureStorageEventStreamBase, IEventStreamIdentity
    {

        // Named additional special attributes for the append blob 
        const string METATDATA_DOMAIN = "DOMAIN";
        const string METADATA_ENTITY_TYPE_NAME = "ENTITYTYPENAME";
        const string METADATA_SEQUENCE = "SEQUENCE";
        const string METADATA_RECORD_COUNT = "RECORDCOUNT";
        const string METADATA_INSTANCE_KEY = "INSTANCEKEY";
        const string METADATA_DATE_CREATED = "DATECREATED";
        const string METADATA_CORRELATION_ID = "CORRELATIONIDENTIFIER";

        // Named specific sub-folders
        const string ORPHANS_FOLDER = "uncategorised";
        const string EVENTSTREAM_FOLDER = "eventstreams";
        const string SNAPSHOTS_FOLDER = "snapshots";

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
                    _blobBasePath = _blobClient.GetContainerReference(GetEventStreamStorageFolderPath(identity.DomainName, identity.EntityTypeName ));
                    // make sure the folder exists 
                    _blobBasePath.CreateIfNotExists();
                }
            }

            _entityTypeName = identity.InstanceKey;
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

            return domainPath + '/' + GetEventStreamStorageFolderPath(entityType);
        }

        public static string GetEventStreamStorageFolderPath( string entityType)
        {
            return EVENTSTREAM_FOLDER + '/' + MakeValidStorageFolderName(entityType);
        }
    }
}
