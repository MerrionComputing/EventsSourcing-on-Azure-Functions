using System;
using System.IO;
using Microsoft.Extensions.Configuration;
using Microsoft.WindowsAzure.Storage;

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
        /// The default connection string to fall back on if nothing else is found
        /// </summary>
        public const string DefaultConnectionStringName = "EventStreamConnectionString";

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
                if (!string.IsNullOrWhiteSpace(connectionStringName))
                { 
                    string connectionString = config.GetConnectionString(connectionStringName);
                    if (string.IsNullOrWhiteSpace(connectionString) )
                    {
                        // Drop back on the storage level connection string
                        connectionString = config.GetConnectionString(StorageConnectionStringSettingName);
                        if (string.IsNullOrWhiteSpace(connectionString))
                        {
                            // Drop back on the default connection string
                            connectionString = config.GetConnectionString(DefaultConnectionStringName);
                        }
                    }
                    _storageAccount = CloudStorageAccount.Parse(connectionString);
                }
            }
        }
    }
}
