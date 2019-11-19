using EventSourcingOnAzureFunctions.Common.Binding;
using EventSourcingOnAzureFunctions.Common.EventSourcing.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
namespace EventSourcingOnAzureFunctions.Common.EventSourcing
{
    /// <summary>
    /// A classifier is a simplified form of a projection that classifies entities as being
    /// in our out of a given group
    /// </summary>
    /// <remarks>
    /// This can be used to implement functionality broardly analoguous to a WHERE clause in SQL
    /// </remarks>
    public class Classification
        : IEventStreamIdentity
    {

        private readonly IEventStreamSettings _settings = null;
        private readonly IClassificationProcessor _classificationProcessor = null;

        /// <summary>
        /// The different states that can result from a 
        /// classifier step process
        /// </summary>
        public enum ClassificationResults
        {
            /// <summary>
            /// The state remains as whatever it was before the classification
            /// </summary>
            Unchanged = 0,
            /// <summary>
            /// The entity instance is marked as being included in the group defined by the classification
            /// </summary>
            Include = 1,
            /// <summary>
            /// The entity instance is marked as being excluded in the group defined by the classification
            /// </summary>
            Exclude = 2
        }

        private readonly string _domainName;
        /// <summary>
        /// The domain in which the event stream the classifier will run over is located
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
        /// The type of entity over which this classifier will run
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
        /// The specific uniquely identitified instance of the entity for which the classifier will run
        /// </summary>
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
        public string ClassifierTypeName
        {
            get
            {
                return _classifierTypeName;
            }
        }


        private readonly string _connectionStringName;
        public string ConnectionStringName
        {
            get
            {
                return _connectionStringName;
            }
        }


        /// <summary>
        /// Create the projection from the attribute linked to the function parameter
        /// </summary>
        /// <param name="attribute">
        /// The attribute describing which projection to run
        /// </param>
        public Classification(ClassificationAttribute attribute,
            IEventStreamSettings settings = null)
        {

            _domainName = attribute.DomainName;
            _entityTypeName = attribute.EntityTypeName;
            _instanceKey = attribute.InstanceKey;
            _classifierTypeName  = attribute.ClassifierTypeName ;


            if (null == settings)
            {
                _settings = new EventStreamSettings();
            }
            else
            {
                _settings = settings;
            }

            _connectionStringName = _settings.GetConnectionStringName(attribute);

            if (null == _classificationProcessor)
            {
                _classificationProcessor = _settings.CreateClassificationProcessorForEventStream(attribute);
            }

        }
    }
}
