using EventSourcingOnAzureFunctions.Common.EventSourcing.Interfaces;
using EventSourcingOnAzureFunctions.Common.EventSourcing.Exceptions;
using EventSourcingOnAzureFunctions.Common.EventSourcing.Interfaces;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EventSourcingOnAzureFunctions.Common.EventSourcing.Implementation.AzureStorage.File
{
    public abstract class FileEventStreamBase
        : AzureStorageEventStreamBase,
        IEventStreamIdentity
    {


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

        /// <summary>
        /// Does a stream already exist for this event stream identity
        /// </summary>
        /// <remarks>
        /// We use the existence of the stream footer record as proof of stream existence
        /// </remarks>
        protected async internal Task<bool> StreamAlreadyExists()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// The name of the default connection string to use for the domain
        /// </summary>
        protected internal string StorageConnectionStringSettingName
        {
            get
            {
                char[] invalidCharacters = @" _!,.;':@£$%^&*()+=/\#~{}[]?<>".ToCharArray();
                return string.Join("", DomainName.Split(invalidCharacters)).Trim() + "FileStorageConnectionString";
            }
        }

        public FileEventStreamBase(IEventStreamIdentity identity,
            bool writeAccess = false,
            string connectionStringName = @"")
            : base(identity.DomainName, writeAccess, connectionStringName)
        {

            _domainName = identity.DomainName;
            _entityTypeName = identity.EntityTypeName;
            _instanceKey = identity.InstanceKey;


            // Set the connection string to use
            if (string.IsNullOrWhiteSpace(connectionStringName))
            {
                connectionStringName = StorageConnectionStringSettingName;
            }


        }



        public static string MakeEventFilename(string DomainName,
            string EntityTypeName,
            string InstanceKey,
            int SequenceNumber = 0)
        {
            string foldername = MakeValidStorageFolderName(DomainName + "/" + EntityTypeName + "/" + InstanceKey);
            return $"{foldername}.event.{SequenceNumber.ToString("0000000000")}";
        }
    }
}
