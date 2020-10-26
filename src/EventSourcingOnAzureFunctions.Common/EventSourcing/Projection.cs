using EventSourcingOnAzureFunctions.Common.Binding;
using EventSourcingOnAzureFunctions.Common.EventSourcing.Interfaces;
using EventSourcingOnAzureFunctions.Common.Notification;
using System;
using System.Linq;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace EventSourcingOnAzureFunctions.Common.EventSourcing
{
    public class Projection
         : IEventStreamIdentity
    {

        private readonly IEventStreamSettings _settings = null;
        private readonly IProjectionProcessor _projectionProcessor = null;
        private readonly INotificationDispatcher _notificationDispatcher = null;
        private readonly IProjectionSnapshotWriter _snapshortWriter = null;
        private readonly IProjectionSnapshotReader _snapshotReader = null;

        private readonly string _domainName;
        /// <summary>
        /// The domain in which this event stream that we are running the projection over is located
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
        /// The type of entity for which this event stream that we are running the projection over pertains
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
        /// The specific uniquely identitified instance of the entity to which this event stream 
        /// that we are running the projection over pertains
        /// </summary>
        public string InstanceKey
        {
            get
            {
                return _instanceKey;
            }
        }

        /// <summary>
        /// The type of the projection we are going to run 
        /// </summary>
        private readonly string _projectionTypeName;
        public string ProjectionTypeName
        {
            get
            {
                return _projectionTypeName;
            }
        }


        public async Task<TProjection> Process<TProjection>(Nullable<DateTime> asOfDate = null) where TProjection : IProjection, new()
        {
            TProjection projectionToRun = new TProjection();
            return await Process(projectionToRun, asOfDate);
        }


        public async Task<TProjection> Process<TProjection>(TProjection projectionToRun, Nullable<DateTime> asOfDate = null) where TProjection : IProjection
        {
            if (null != _projectionProcessor)
            {
                TProjection ret = await _projectionProcessor.Process<TProjection>(projectionToRun, asOfDate);
                if (null != _notificationDispatcher)
                {
                    // Dispatch a projection-completed notification
                    await _notificationDispatcher.ProjectionCompleted(this,
                        ProjectionNameAttribute.GetProjectionName(typeof(TProjection)),
                        ret.CurrentSequenceNumber,
                        asOfDate,
                        ret);
                }
                return ret;
            }
            else
            {
                return await Task.FromException<TProjection>(new Exception("Projection processor not initialised"));
            }
        }


        /// <summary>
        /// Save the current state of the projection as a snapshot
        /// </summary>
        /// <typeparam name="TProjection">
        /// The type of the projection for which the snapshot is being taken
        /// </typeparam>
        /// <param name="snapshot">
        /// The effective snapshot as of which the snapshot is being taken
        /// </param>
        /// <param name="state">
        /// The state of the projection when snapshotted
        /// </param>
        public async Task WriteSnapshot<TProjection>(ISnapshot snapshot, TProjection state) where TProjection : IProjection
        {
            // If there is a snapshot writer, use it...
            if (null != _snapshortWriter)
            {
                await _snapshortWriter.WriteSnapshot(snapshot, state); 
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
        /// Does the underlying event stream for this projection exist ?
        /// </summary>
        public async Task<bool> Exists()
        {
            if (null != _projectionProcessor)
            {
                return await _projectionProcessor.Exists();
            }
            return false;
        }

        /// <summary>
        /// Create the projection from the attribute linked to the function parameter
        /// </summary>
        /// <param name="attribute">
        /// The attribute describing which projection to run
        /// </param>
        public Projection(ProjectionAttribute attribute,
            IEventStreamSettings settings = null,
            INotificationDispatcher dispatcher = null,
            IProjectionSnapshotReader snapshotReader = null,
            IProjectionSnapshotWriter snapshotWriter = null)
        {

            _domainName = attribute.DomainName;
            _entityTypeName  = attribute.EntityTypeName ;
            _instanceKey = attribute.InstanceKey;
            _projectionTypeName = attribute.ProjectionTypeName;


            if (null == settings)
            {
                _settings = new EventStreamSettings();
            }
            else
            {
                _settings = settings;
            }

            _connectionStringName = _settings.GetConnectionStringName(attribute);

            if (null == _projectionProcessor)
            {
                _projectionProcessor = _settings.CreateProjectionProcessorForEventStream(attribute);
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
                _snapshortWriter = snapshotWriter;
            }

        }


    }
}
