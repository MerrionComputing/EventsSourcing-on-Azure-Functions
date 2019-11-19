using EventSourcingOnAzureFunctions.Common.Binding;
using System;
using System.Collections.Generic;
using System.Text;

namespace EventSourcingOnAzureFunctions.Common.EventSourcing.Interfaces
{
    public interface IEventStreamSettings
    {

        /// <summary>
        /// Load the settings from the application configuration
        /// </summary>
        void LoadFromConfig(string basePath = null);

        /// <summary>
        /// Returns the name of the type of backing store used to store this event stream instance
        /// </summary>
        /// <param name="attribute">
        /// The attribute that defines the event stream instance we are using
        /// </param>
        string GetBackingImplementationType(IEventStreamIdentity attribute);


        /// <summary>
        /// Returns the name of the connection string to use to connect to this event stream instance
        /// </summary>
        /// <param name="attribute">
        /// The attribute that defines the event stream instance we are using
        /// </param>
        string GetConnectionStringName(IEventStreamIdentity attribute);

        /// <summary>
        /// Create a writer for the given event stream identity
        /// </summary>
        /// <param name="attribute">
        /// The event stream to write to
        /// </param>
        IEventStreamWriter CreateWriterForEventStream(IEventStreamIdentity attribute);

        /// <summary>
        /// Create a projection processor for the given event stream and projection
        /// </summary>
        /// <param name="attribute">
        /// The unique identity of the event stream and the projection to run over it
        /// </param>
        IProjectionProcessor CreateProjectionProcessorForEventStream(ProjectionAttribute attribute);

        /// <summary>
        /// Create a classification processor for the given event stream and classifier
        /// </summary>
        /// <param name="attribute">
        /// The unique identity of the event stream and the projection to run over it
        /// </param>
        IClassificationProcessor CreateClassificationProcessorForEventStream(ClassificationAttribute attribute);
    }
}
