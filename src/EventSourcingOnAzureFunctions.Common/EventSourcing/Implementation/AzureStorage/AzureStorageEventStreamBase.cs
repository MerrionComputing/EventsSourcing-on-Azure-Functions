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

        /// <summary>
        /// The default folder where uncateggorised entities are stored
        /// </summary>
        public const string ORPHANS_FOLDER = "uncategorised";

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

            if (string.IsNullOrWhiteSpace(rawName))
            {
                return ORPHANS_FOLDER;
            }


            char[] invalidCharacters = @" _!,.;':@£$%^&*()+=/\#~{}[]?<>".ToCharArray();
            string cleanName = string.Join('-', rawName.Split(invalidCharacters));

            if (cleanName.StartsWith('-'))
            {
                cleanName = cleanName.TrimStart('-');
            }

            if (cleanName.EndsWith('-'))
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
    }
}
