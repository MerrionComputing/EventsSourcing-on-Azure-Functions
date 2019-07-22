using EventSourcingOnAzureFunctions.Common.EventSourcing.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace EventSourcingOnAzureFunctions.Common.Binding
{
    /// <summary>
    /// The connection string to use for this aggrgeate class
    /// </summary>
    /// <remarks>
    /// If not set a default one is used based on the domain name and aggregate type
    /// </remarks>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public sealed class ConnectionStringNameAttribute
        : Attribute 
    {

        private readonly string _connectionStringName;

        /// <summary>
        /// The name of the connection string to use when persisting events for this entity to its event stream
        /// </summary>
        public string Name
        {
            get
            {
                return _connectionStringName;
            }
        }


        public ConnectionStringNameAttribute(string name)
        {
            _connectionStringName = name;
        }


        public static string DefaultConnectionStringName(IEventStreamIdentity entity)
        {
            return DefaultConnectionStringName(entity.DomainName, entity.EntityTypeName);
        }

        public static string DefaultConnectionStringName(string domainName, string entityTypeName)
        {
            // TODO : First see if there is a mapping we can use...
#if TODO
            foreach (AggregateTypeMapping mapping in AggregateTypeMapping.ConfiguredAggregateTypeMappings)
            {
                if (mapping.Matches(domainName, entityTypeName))
                {
                    if (mapping.StorageType == AggregateTypeMapping.BackingStorageType.BlobStream)
                    {
                        return mapping.BlobStreamSettings.ConnectionStringName;
                    }
                }
            }
#endif

            // If not, make the connection string name directly from the domain and aggregate name
            if (!string.IsNullOrWhiteSpace(entityTypeName))
            {
                return $"{domainName}.{entityTypeName}.StorageConnectionString";
            }
            else
            {
                return $"{domainName}.StorageConnectionString";
            }
        }
    }
}
