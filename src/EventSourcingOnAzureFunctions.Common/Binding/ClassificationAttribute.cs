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

        
        private readonly string _classifierTypeName;
        /// <summary>
        /// The specific classifier type to execute
        /// </summary>
        [AutoResolve]
        public string ClassifierTypeName
        {
            get
            {
                return _classifierTypeName;
            }
        }

        private readonly string _notificationDispatcherName;
        /// <summary>
        /// The notification dispatcher to use to send out notifications whenever a classification completes
        /// </summary>
        [AutoResolve]
        public string NotificationDispatcherName
        {
            get
            {
                return _notificationDispatcherName;
            }
        }


        /// <summary>
        /// Creates a new attribute to identify a class that performs a classification function over an event stream
        /// </summary>
        /// <param name="domainName">
        /// The domain of the event stream over which the classifier will be run
        /// </param>
        /// <param name="entityTypeName">
        /// The entity type of the event stream over which the classifier will be run
        /// </param>
        /// <param name="instanceKey">
        /// The unique entity instance of the event stream over which the classifier will be run
        /// </param>
        /// <param name="classifierTypeName">
        /// The name of the classifier process
        /// </param>
        /// <param name="notificationDispatcherName">
        /// The name of the notification dispatcher used to notify the results when the classifier has run
        /// </param>
        public ClassificationAttribute(string domainName,
                        string entityTypeName,
                        string instanceKey,
                        string classifierTypeName,
                        string notificationDispatcherName = "")
        {
            _domainName = domainName;
            _entityTypeName = entityTypeName;
            _instanceKey = instanceKey;
            _classifierTypeName = classifierTypeName;
            _notificationDispatcherName = notificationDispatcherName;
        }
    }
}
