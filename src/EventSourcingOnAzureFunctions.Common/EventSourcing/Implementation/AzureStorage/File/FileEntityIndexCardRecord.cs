using EventSourcingOnAzureFunctions.Common.EventSourcing.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EventSourcingOnAzureFunctions.Common.EventSourcing.Implementation.AzureStorage.File
{
    /// <summary>
    /// An index card record that is used to tell when a given entity came into being
    /// </summary>
    public sealed class FileEntityIndexCardRecord
        : IEventStreamIdentity
    {

        private string _domainName;
        /// <summary>
        /// The domain in which this event stream is set
        /// </summary>
        public string DomainName
        {
            get
            {
                return _domainName;
            }
            set
            {
                _domainName = value;
            }
        }

        private string _entityTypeName;
        /// <summary>
        /// The type of entity for which this event stream pertains
        /// </summary>
        public string EntityTypeName
        {
            get
            {
                return _entityTypeName;
            }
            set
            {
                _entityTypeName = value;
            }
        }

        private string _instanceKey;
        /// <summary>
        /// The specific uniquely identitified instance of the entity to which this event stream pertains
        /// </summary>
        public string InstanceKey
        {
            get
            {
                return _instanceKey;
            }
            set
            {
                _instanceKey = value;
            }
        }

    }
}
