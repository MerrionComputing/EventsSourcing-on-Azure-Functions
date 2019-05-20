using Microsoft.Azure.Storage.Blob;
using System;
using System.Collections.Generic;
using System.Text;

namespace EventSourcingOnAzureFunctions.Common.EventSourcing.Implementation.AzureStorage.AppendBlob
{
    public class BlobEventStreamBase
        : AzureStorageEventStreamBase
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


        private readonly CloudBlobClient _blobClient;


        public BlobEventStreamBase(string domainName,
            bool writeAccess = false,
            string connectionStringName = @"")
            : base(domainName , writeAccess, connectionStringName )
        {

            if (null != _storageAccount)
            {
                _blobClient = _storageAccount.CreateCloudBlobClient();
                if (null != _blobClient)
                {
                    // Create the reference to this aggretate type's event stream base folder
                    // e.g. /[domain name]/

                }
            }

        }

        
    }
}
