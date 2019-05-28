using System;
using System.IO;
using Microsoft.Azure.Storage;
using Microsoft.Extensions.Configuration;


namespace EventSourcingOnAzureFunctions.Common.EventSourcing.Implementation.AzureStorage
{
    /// <summary>
    ///  Common functionality that both reader and writer use to access any event store based on Azure storage
    /// </summary>
    public class AzureStorageEventStreamBase
        : EventStreamBase
    {

        private readonly string _domainName;
        protected internal CloudStorageAccount _storageAccount;

        /// <summary>
        /// The domain name in which this event stream is residing
        /// </summary>
        public string DomainName
        {
            get { return _domainName; }
        }

        /// <summary>
        /// The name of the default connection string to use for the domain
        /// </summary>
        protected internal string StorageConnectionStringSettingName
        {
            get
            {
                char[] invalidCharacters = @" _!,.;':@£$%^&*()+=/\#~{}[]?<>".ToCharArray ();
                return string.Join("", DomainName.Split(invalidCharacters)).Trim() + "StorageConnectionString";
            }
        }


        public AzureStorageEventStreamBase(string domainName,
            bool writeAccess = false,
            string connectionStringName = @"")
        {

            _domainName = domainName;

            // Set the connection string to use
            if (string.IsNullOrWhiteSpace(connectionStringName ) )
            {
                connectionStringName = StorageConnectionStringSettingName;
            }

            // Create a connection to the cloud storage account to use
            ConfigurationBuilder builder = new ConfigurationBuilder();
            builder.SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", true )
                .AddJsonFile("config.local.json", true)
                .AddJsonFile("config.json", true)
                .AddJsonFile("connectionstrings.json", true)
                .AddEnvironmentVariables();

            IConfigurationRoot config = builder.Build();

            if (null != config)
            {
                if (! string.IsNullOrWhiteSpace(connectionStringName) )
                {
                    _storageAccount = CloudStorageAccount.Parse(config.GetConnectionString(connectionStringName));
                }
            }
        }
    }
}
