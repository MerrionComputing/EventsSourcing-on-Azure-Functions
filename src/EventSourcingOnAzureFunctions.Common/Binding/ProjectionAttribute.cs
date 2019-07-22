using System;

using EventSourcingOnAzureFunctions.Common.EventSourcing.Interfaces;
using Microsoft.Azure.WebJobs.Description;

namespace EventSourcingOnAzureFunctions.Common.Binding
{

    /// <summary>
    /// An attribute to mark a projection to get state information from an event stream by projection
    /// </summary>
    [AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false)]
    [Binding]
    public sealed class ProjectionAttribute
        : Attribute, IEventStreamIdentity
    {

        private readonly string _domainName;
        /// <summary>
        /// The domain in which the event stream the projection will run over is located
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
        /// The type of entity over which this projection will run
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
        /// The specific uniquely identitified instance of the entity for which the projection will run
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
        /// The specific projection type to execute
        /// </summary>
        private readonly string _projectionTypeName;
        [AutoResolve]
        public string ProjectionTypeName
        {
            get
            {
                return _projectionTypeName;
            }
        }

        // Note: The parameter names need to match the property names (except for the camelCase) because the autoresolve
        // uses this fact to perform the instatntiation
        public ProjectionAttribute(string domainName,
                        string entityTypeName,
                        string instanceKey,
                        string projectionTypeName)
        {
            _domainName = domainName;
            _entityTypeName  = entityTypeName;
            _instanceKey = instanceKey;
            _projectionTypeName = projectionTypeName;
        }
    }
}
