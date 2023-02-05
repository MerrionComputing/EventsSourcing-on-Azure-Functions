using EventSourcingOnAzureFunctions.Common.Binding;
using EventSourcingOnAzureFunctions.Common.EventSourcing.Interfaces;
using EventSourcingOnAzureFunctions.Common.Notification;
using System;
using System.Collections.Generic;
using System.Linq;
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
        private readonly INotificationDispatcher _notificationDispatcher = null;
        private readonly IClassificationSnapshotReader _snapshotReader = null;
        private readonly IClassificationSnapshotWriter _snapshotWriter = null;
               
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
        /// Set a parameter to be used when running the classifier
        /// </summary>
        /// <param name="parameterName">
        /// The name of the parameter - this must be unique per classifier
        /// </param>
        /// <param name="parameterValue">
        /// The value to use for that named parameter for this run
        /// </param>
        public void SetParameter(string parameterName, object parameterValue)
        {
            if (null != _classificationProcessor )
            {
                _classificationProcessor.SetParameter(parameterName, parameterValue);  
            }
        }

        public async Task<ClassificationResponse> Classify<TClassification>(DateTime? asOfDate = null) where TClassification : IClassification, new()
        {
            TClassification classificationToRun = new TClassification();
            return await Classify(classificationToRun, asOfDate);
        }

        public async Task<ClassificationResponse> Classify(IClassification classificationToRun, 
            DateTime? asOfDate = null)
        {
            if (null != _classificationProcessor)
            {
                ClassificationResponse ret = await _classificationProcessor.Classify(classificationToRun,
                    asOfDate);
                if (null != _notificationDispatcher)
                {
                    await _notificationDispatcher.ClassificationCompleted(this,
                        ClassificationNameAttribute.GetClassificationName(classificationToRun.GetType()),
                        _classificationProcessor.Parameters,
                        ret.AsOfSequence,
                        asOfDate,
                        ret);
                }
                return ret;
            }
            else
            {
                return await Task.FromException<ClassificationResponse>(
                    new Exception("Classification processor not initialised"));
            }
        }


        /// <summary>
        /// Get all of the unique instances of this domain/entity type
        /// </summary>
        /// <param name="asOfDate">
        /// (Optional) The date as of which to get all the instance keys
        /// </param>
        /// <remarks>
        /// This is to allow for set-based functionality
        /// </remarks>
        public async Task<IEnumerable<string > > GetAllInstanceKeys(DateTime? asOfDate = null)
        {
            if (null != _classificationProcessor)
            {
                return await _classificationProcessor.GetAllInstanceKeys(asOfDate);
            }
            else
            {
                return await Task.FromException<IEnumerable<string>>(new Exception("Classification processor not initialised"));
            }
        }

        /// <summary>
        /// Create the projection from the attribute linked to the function parameter
        /// </summary>
        /// <param name="attribute">
        /// The attribute describing which projection to run
        /// </param>
        public Classification(ClassificationAttribute attribute,
            IEventStreamSettings settings = null,
            INotificationDispatcher dispatcher = null,
            IClassificationSnapshotReader snapshotReader = null,
            IClassificationSnapshotWriter snapshotWriter = null)
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

            if (null == dispatcher)
            {
                // Create a new dispatcher 
                _notificationDispatcher = NotificationDispatcherFactory.GetDefaultDispatcher();
            }
            else
            {
                _notificationDispatcher = dispatcher;
            }

            if (null != snapshotReader )
            {
                _snapshotReader = snapshotReader;
            }

            if (null != snapshotWriter )
            {
                _snapshotWriter = snapshotWriter;
            }
        }
    }


    public class  ClassificationResponse
    {
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

        private readonly ClassificationResults _result;
        /// <summary>
        /// The result of the classification 
        /// </summary>
        public ClassificationResults Result
        {
            get
            {
                return _result;
            }
        }

        private readonly int _asOfSequence;
        /// <summary>
        /// The last sequence number read to get this classification result
        /// </summary>
        public int AsOfSequence
        {
            get
            {
                return _asOfSequence;
            }
        }

        private readonly DateTime? _asOfDate;
        public DateTime? AsOfDate
        {
            get
            {
                return _asOfDate;
            }
        }

        private  readonly bool _wasEverIncluded = false;
        /// <summary>
        /// Was this entity ever included according to this classifier
        /// </summary>
        public bool WasEverIncluded
        {
            get
            {
                return _wasEverIncluded;
            }
        }

        private readonly bool _wasEverExcluded = false;
        /// <summary>
        /// Was this entity ever excluded according to this classifier
        /// </summary>
        public bool WasEverExcluded
        {
            get
            {
                return _wasEverExcluded;
            }
        }

        private readonly Dictionary<string, object> _parameters = new Dictionary<string, object>();
        public Dictionary<string, object> Parameters
        {
            get
            {
                return _parameters;
            }
        }

        public ClassificationResponse(ClassificationResults result,
            int asofSequence,
            DateTime? asOfDate,
            bool wasEverIncluded,
            bool wasEverExcluded,
            Dictionary<string, object> parameters = null)
        {

            _result = result;
            _asOfSequence = asofSequence;
            _asOfDate = asOfDate;

            _wasEverIncluded = wasEverIncluded;
            _wasEverExcluded = wasEverExcluded;

            if (_result == ClassificationResults.Include )
            {
                _wasEverIncluded = true;
            }

            if (_result == ClassificationResults.Exclude )
            {
                _wasEverExcluded = true;
            }

            if (null != parameters )
            {
                _parameters = parameters;
            }
        }
    }
}
