using System;

using EventSourcingOnAzureFunctions.Common.EventSourcing.Interfaces;
using Microsoft.Azure.WebJobs.Description;

namespace EventSourcingOnAzureFunctions.Common.Binding
{
    /// <summary>
    /// An attribute to mark a classifier to get group membership classification information from an event stream by projection
    /// </summary>
    [AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false)]
    [Binding]
    public class ClassificationAttribute
        : Attribute, IEventStreamIdentity
    {

        private readonly string _domainName;
        /// <summary>
        /// The domain in which the event stream the classifier will run over is located
        /// </summary>
        [AutoResolve]
        public string DomainName
        {
            get
            {
                return _domainName;
            }
        }


        private readonly string _entityTypeName;
        /// <summary>
        /// The type of entity over which this classifier will run
        /// </summary>
        [AutoResolve]
        public string EntityTypeName
        {
            get
            {
                return _entityTypeName;
            }
        }

        private readonly string _instanceKey;
        /// <summary>
        /// The specific uniquely identitified instance of the entity for which the classifier will run
        /// </summary>
        [AutoResolve]
        public string InstanceKey
        {
            get
            {
                return _instanceKey;
            }
        }

        /// <summary>
        /// The specific classifier type to execute
        /// </summary>
        private readonly string _classifierTypeName;
        [AutoResolve]
        public string ClassifierTypeName
        {
            get
            {
                return _classifierTypeName;
            }
        }

        // Note: The parameter names need to match the property names (except for the camelCase) because 
        // the autoresolve uses this fact to perform the instatntiation
        public ClassificationAttribute(string domainName,
                        string entityTypeName,
                        string instanceKey,
                        string classifierTypeName)
        {
            _domainName = domainName;
            _entityTypeName = entityTypeName;
            _instanceKey = instanceKey;
            _classifierTypeName = classifierTypeName;
        }
    }
}
