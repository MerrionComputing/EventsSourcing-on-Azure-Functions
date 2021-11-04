using EventSourcingOnAzureFunctions.Common.Binding;
using EventSourcingOnAzureFunctions.Common.ClassifierHandler.Projections;
using EventSourcingOnAzureFunctions.Common.CQRS.ClassifierHandler.Events;
using EventSourcingOnAzureFunctions.Common.CQRS.Common.Events;
using EventSourcingOnAzureFunctions.Common.CQRS.ProjectionHandler.Events;
using EventSourcingOnAzureFunctions.Common.CQRS.ProjectionHandler.Projections;
using EventSourcingOnAzureFunctions.Common.CQRS.QueryHandler.Events;
using EventSourcingOnAzureFunctions.Common.EventSourcing;
using EventSourcingOnAzureFunctions.Common.EventSourcing.Implementation;
using EventSourcingOnAzureFunctions.Common.EventSourcing.Interfaces;
using EventSourcingOnAzureFunctions.Common.Notification;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static EventSourcingOnAzureFunctions.Common.EventSourcing.ClassificationResponse;

namespace EventSourcingOnAzureFunctions.Common.CQRS
{
    /// <summary>
    /// A wrapper class used to trigger and manage a query orchestration
    /// </summary>
    public class Query
    {

        private readonly string _queryDispatcherName = nameof(QueueNotificationDispatcher);
        private readonly IWriteContext _queryContext;

        private readonly string _domainName;
        /// <summary>
        /// The business domain in which the query is located
        /// </summary>
        public string DomainName
        {
            get
            {
                return _domainName;
            }
        }

        private readonly string _queryName;
        /// <summary>
        /// The name of the query to run
        /// </summary>
        public string QueryName
        {
            get
            {
                return _queryName;
            }
        }

        private readonly string _uniqueIdentifier;
        /// <summary>
        /// The unique instance of the query to run
        /// </summary>
        public string UniqueIdentifier
        {
            get
            {
                return _uniqueIdentifier;
            }
        }

        /// <summary>
        /// Create the new query orchestration to be processed
        /// </summary>
        /// <param name="ExternalSystemUniqueIdentifier">
        /// (Optional) A unique identifier for this query as passed in by an external system
        /// </param>
        /// <param name="AuthorisationToken">
        /// (Optional) A token to use for authorisation(s) checking within this query
        /// </param>
        /// <param name="ExternalOrchestrationIdentifier">
        /// (optional) An unique identifier to use if this query is being orchestrated by an external
        /// system which uses its own provided unique identifiers
        /// </param>
        /// <returns></returns>
        public async Task Create(
            string ExternalSystemUniqueIdentifier = "",
            string AuthorisationToken = "",
            string ExternalOrchestrationIdentifier = "")
        {

            EventStream esQry = new EventStream(new EventStreamAttribute(MakeDomainQueryName(DomainName),
                QueryName,
                UniqueIdentifier,
                notificationDispatcherName: _queryDispatcherName),
                context: _queryContext);

            Created evCreated = new Created()
            {
                AuthorisationToken = AuthorisationToken,
                ExternalOrchestrationIdentifier = ExternalOrchestrationIdentifier,
                ExternalSystemUniqueIdentifier = ExternalSystemUniqueIdentifier,
                DateLogged = DateTime.UtcNow
            };

            await esQry.AppendEvent(evCreated);
        }

        /// <summary>
        /// Set a parameter to be used for the query
        /// </summary>
        /// <param name="parameterName">
        /// The name of the query parameter
        /// </param>
        /// <param name="parameterValue">
        /// The value to use for that parameter
        /// </param>
        public async Task SetParameter(string parameterName, object parameterValue)
        {
            if (!string.IsNullOrWhiteSpace(parameterName))
            {

                EventStream esQry = new EventStream(new EventStreamAttribute(MakeDomainQueryName(DomainName),
                    QueryName,
                    UniqueIdentifier,
                    notificationDispatcherName: _queryDispatcherName),
                    context: _queryContext);

                ParameterValueSet evParam = new ParameterValueSet()
                {
                    Name = parameterName,
                    Value = parameterValue
                };

                await esQry.AppendEvent(evParam);
            }
        }

        // Validations

        // Classification request
        /// <summary>
        /// Request a classification to be performed as part of this query
        /// </summary>
        /// <param name="domainName">
        /// The domain name of the entity over which the classification is to run
        /// </param>
        /// <param name="entityTypeName">
        /// The entity type over which to run the classification
        /// </param>
        /// <param name="instanceKey">
        /// The specific instance over which to run the classification
        /// </param>
        /// <param name="classifierTypeName">
        /// The specific type of classification process to run over the event stream
        /// </param>
        /// <param name="asOfDate">
        /// (Optional) The date up to which to run the classification
        /// </param>
        /// <param name="classificationParameters">
        /// (Optional) Any additional parameters to use in the classification process
        /// </param>
        public async Task RequestClassification(string domainName,
                        string entityTypeName,
                        string instanceKey,
                        string classifierTypeName,
                        Nullable<DateTime> asOfDate ,
                        IEnumerable<KeyValuePair<string, object >> classificationParameters )
        {

            // Correlation to link the parameters to the classification 
            Guid correlationId = Guid.NewGuid();

            EventStream esQry = new EventStream(new EventStreamAttribute(MakeDomainQueryName(DomainName),
                QueryName,
                UniqueIdentifier,
                notificationDispatcherName: _queryDispatcherName),
                context: _queryContext);

            if (null != classificationParameters )
            {
                // add a classification request parameter for each...
                foreach (KeyValuePair<string, object>  parameter in classificationParameters )
                {
                    ClassifierRequestParameterSet evParam = new ClassifierRequestParameterSet()
                    {
                        CorrelationIdentifier = correlationId.ToString(),
                        ParameterName = parameter.Key ,
                        ParameterValue = parameter.Value 
                    };

                    await esQry.AppendEvent(evParam);
                }
            }

            // add the classification request
            ClassifierRequested evRequest = new ClassifierRequested()
            {
                CorrelationIdentifier = correlationId.ToString(),
                DomainName = domainName ,
                EntityTypeName = entityTypeName ,
                InstanceKey = instanceKey ,
                ClassifierTypeName = classifierTypeName ,
                AsOfDate = asOfDate 
            };

            await esQry.AppendEvent(evRequest);  

        }

        // Classifier response...
        /// <summary>
        /// Post a response from a classifier onto the given query event stream
        /// </summary>
        /// <param name="domainName">
        /// The domain name of the entity over which the classification was run
        /// </param>
        /// <param name="entityTypeName">
        /// The entity type over which the classification was run
        /// </param>
        /// <param name="instanceKey">
        /// The specific instance over which the classification was run
        /// </param>
        /// <param name="classifierTypeName">
        /// The specific type of classification process to run over the event stream
        /// </param>
        /// <param name="asOfDate">
        /// (Optional) The date up to which to run the classification
        /// </param>
        /// <param name="correlationIdentifier">
        /// The unique identifier for this classification instance
        /// </param>
        /// <param name="asOfSequenceNumber">
        /// The sequence number of the last event read when running the classifier
        /// (This can be used to determine if the classification is still valid)
        /// </param> 
        /// <param name="result">
        /// The result of running the classifier
        /// </param>
        /// <returns></returns>
        public async Task PostClassifierResponse(string domainName,
                        string entityTypeName,
                        string instanceKey,
                        string classifierTypeName,
                        Nullable<DateTime> asOfDate,
                        string correlationIdentifier,
                        int asOfSequenceNumber,
                        ClassificationResults result)
        {

            EventStream esQry = new EventStream(new EventStreamAttribute(MakeDomainQueryName(DomainName),
                QueryName,
                UniqueIdentifier,
                notificationDispatcherName: _queryDispatcherName),
                context: _queryContext);

            ClassifierResultReturned evRet = new ClassifierResultReturned()
            {
                DomainName = domainName ,
                EntityTypeName = entityTypeName ,
                InstanceKey = instanceKey ,
                ClassifierTypeName = classifierTypeName ,
                AsOfDate = asOfDate ,
                AsOfSequenceNumber = asOfSequenceNumber ,
                CorrelationIdentifier = correlationIdentifier,
                Result = result 
            };


            await esQry.AppendEvent(evRet); 
        }

        /// <summary>
        /// Gets the set of projection requests outstanding for this query 
        /// </summary>
        /// <returns>
        /// The set of projection requests with no matching response
        /// </returns>
        public async Task<IEnumerable<IClassifierRequest>> GetOutstandingClassifiers()
        {
            // Run the [Outstanding Classifications] projection over this query's event stream.
            Projection outstanding = new Projection(new ProjectionAttribute(MakeDomainQueryName(DomainName),
                QueryName,
                UniqueIdentifier,
                ProjectionNameAttribute.GetProjectionName(typeof(OutstandingClassifications)),
                notificationDispatcherName: _queryDispatcherName)
                );

            var outstandingClassifications = await outstanding.Process<OutstandingClassifications>();

            if (outstandingClassifications != null)
            {
                return outstandingClassifications.ClassificationsToBeProcessed;
            }

            // If nothing found return an empty set for composability
            return Enumerable.Empty<IClassifierRequest>();
        }

        // Projection request
        /// <summary>
        /// Request a projection to be performed
        /// </summary>
        /// <param name="domainName">
        /// The domain name of the entity over which the classification is to run
        /// </param>
        /// <param name="entityTypeName">
        /// The entity type over which to run the classification
        /// </param>
        /// <param name="instanceKey">
        /// The specific instance over which to run the classification
        /// </param>
        /// <param name="classifierTypeName">
        /// The specific type of classification process to run over the event stream
        /// </param>
        /// <param name="asOfDate">
        /// (Optional) The date up to which to run the classification
        /// </param>
        public async Task RequestProjection(string domainName,
                    string entityTypeName,
                    string instanceKey,
                    string projectionTypeName,
                    Nullable<DateTime> asOfDate = null)
        {
            Guid correlationId = Guid.NewGuid();

            EventStream esQry = new EventStream(new EventStreamAttribute(
                       MakeDomainQueryName(DomainName),
                       QueryName,
                       UniqueIdentifier,
                       notificationDispatcherName: _queryDispatcherName),
                       context: _queryContext);

            ProjectionRequested evPrj = new ProjectionRequested()
            {
                CorrelationIdentifier = correlationId.ToString(),
                ProjectionDomainName = domainName,
                ProjectionEntityTypeName = entityTypeName,
                ProjectionInstanceKey = instanceKey,
                ProjectionTypeName = projectionTypeName,
                AsOfDate = asOfDate,
                DateLogged = DateTime.UtcNow
            };

            await esQry.AppendEvent(evPrj);
        }

        /// <summary>
        /// Post the response from running the requested projection onto the query 
        /// event stream so it can be used for further processing
        /// </summary>
        /// <param name="domainName">
        /// The domain the projection was run in
        /// </param>
        /// <param name="entityTypeName">
        /// The entity type for which the projection was run
        /// </param>
        /// <param name="instanceKey">
        /// The unique identifier of the entity for which the projection was run
        /// </param>
        /// <param name="projectionTypeName">
        /// The type of projection that was run over that entity
        /// </param>
        /// <param name="asOfDate">
        /// The as-of date for which the projection response was valid
        /// </param>
        /// <param name="correlationIdentifier">
        /// Unique identifier correlating projection requests/responses
        /// </param>
        /// <param name="asOfSequenceNumber">
        /// The sequence number of the last event read running this projection
        /// </param>
        /// <param name="projectionResult">
        /// The actual result of running the projection
        /// </param>
        public async Task PostProjectionResponse(string domainName,
                        string entityTypeName,
                        string instanceKey,
                        string projectionTypeName,
                        Nullable<DateTime> asOfDate, 
                        string correlationIdentifier,
                        int asOfSequenceNumber,
                        object projectionResult)
        {

            EventStream esQry = new EventStream(new EventStreamAttribute(MakeDomainQueryName(DomainName),
                QueryName,
                UniqueIdentifier,
                notificationDispatcherName: _queryDispatcherName),
                context: _queryContext);

            ProjectionValueReturned evRet = new ProjectionValueReturned()
            {
                ProjectionDomainName = domainName ,
                ProjectionEntityTypeName = entityTypeName,
                ProjectionInstanceKey = instanceKey ,
                AsOfDate = asOfDate ,
                AsOfSequenceNumber = asOfSequenceNumber ,
                CorrelationIdentifier = correlationIdentifier ,
                ProjectionTypeName = projectionTypeName ,
                DateLogged = DateTime.UtcNow ,
                Value = projectionResult.ToString() 
            };

            await esQry.AppendEvent(evRet);  
        }


        /// <summary>
        /// Gets the set of projection requests outstanding for this query 
        /// </summary>
        /// <returns>
        /// The set of projection requests with no matching response
        /// </returns>
        public async Task<IEnumerable<IProjectionRequest >> GetOutstandingProjections()
        {

            // Run the [Outstanding Projections] projection over this query's event stream.
            Projection outstanding = new Projection(new ProjectionAttribute(MakeDomainQueryName(DomainName),
                QueryName,
                UniqueIdentifier,
                ProjectionNameAttribute.GetProjectionName(typeof (OutstandingProjections)),
                notificationDispatcherName: _queryDispatcherName)
                );

            var outstandingProjections = await outstanding.Process<OutstandingProjections>();

            if (outstandingProjections!= null)
            {
                return outstandingProjections.ProjectionsToBeProcessed;
            }

            // If nothing found return an empty set for composability
            return Enumerable.Empty<IProjectionRequest>();
        }

        // Collations

        /// <summary>
        /// Run a projection over this query event stream to give the collated results
        /// </summary>
        /// <typeparam name="TProjectionResult">
        /// The data type of the collation result
        /// </typeparam>
        /// <param name="collationProjectionName">
        /// The projection name to perform the collation function
        /// </param>
        public async Task<TProjectionResult> RunCollationProjection<TProjectionResult>(string collationProjectionName ) where TProjectionResult : IProjection , new()
        {

            if (!string.IsNullOrEmpty(collationProjectionName))
            {
                Projection collating = new Projection(new ProjectionAttribute(MakeDomainQueryName(DomainName),
                    QueryName,
                    UniqueIdentifier,
                    collationProjectionName,
                    notificationDispatcherName: _queryDispatcherName)
                    );

                var collationResult = await collating.Process<TProjectionResult>();

                return collationResult;

            }

            // If we got here then no collation result was possible
            return default(TProjectionResult);

        }

        // Response
        private async Task SetResponseTarget(string targetType,
            string targetLocation)
        {
            if ((!string.IsNullOrWhiteSpace(targetType )) && (!string.IsNullOrWhiteSpace(targetLocation )))
            {

                EventStream esQry = new EventStream(new EventStreamAttribute(MakeDomainQueryName(DomainName),
                    QueryName,
                    UniqueIdentifier,
                    notificationDispatcherName: _queryDispatcherName),
                    context: _queryContext);

                OutputLocationSet evParam = new OutputLocationSet()
                {
                    TargetType  = targetType ,
                    Location  = targetLocation 
                };

                await esQry.AppendEvent(evParam);
            }
        }

        /// <summary>
        /// Add an azure storage blob target for the query to save its results
        /// into
        /// </summary>
        public async Task AddStorageBlobOutput(Uri containerAddress,
            Uri targetFilename)
        {
            await SetResponseTarget("Azure Storage Blob",
                new Uri(containerAddress, targetFilename).AbsoluteUri); 
        }

        /// <summary>
        /// Add an email output target for the query results
        /// </summary>
        /// <param name="emailAddress">
        /// The address to which to send the query results
        /// </param>
        public async Task AddEmailOutput(string emailAddress)
        {
            await SetResponseTarget("Email Address",
                emailAddress );
        }

        /// <summary>
        /// Add a webhook output target for the query results
        /// </summary>
        /// <param name="webhookAddress">
        /// The URI of the web hook to which to post results
        /// </param>
        public async Task AddWebhookOutput(Uri webhookAddress)
        {
            await SetResponseTarget("Webhook",
                webhookAddress.AbsoluteUri );
        }

        /// <summary>
        /// Send the output of a query to Event Grid for distribution 
        /// </summary>
        /// <param name="eventType">
        /// The event type to use for the event
        /// </param>
        /// <param name="subject">
        /// The subject to use to route the event with
        /// </param>
        /// <param name="eventGridTopicEndpoint">
        /// The end point to send the event grid message out to
        /// </param>
        public async Task AddEventGridOutput(string eventType,
            string subject,
            Uri eventGridTopicEndpoint)
        {
            EventGridEventRouting er = new EventGridEventRouting()
            { 
                EventType = eventType,
                Subject = subject,
                EventGridTopicEndpoint = eventGridTopicEndpoint.AbsolutePath  
            };

            string jsonRouting = JsonConvert.SerializeObject(er);

            await SetResponseTarget("Event Grid Event",
                jsonRouting); 

        }

        // Cleanup
        /// <summary>
        /// Delete the query backing event stream
        /// </summary>
        public async Task Delete()
        {
            EventStream esQry = new EventStream(new EventStreamAttribute(MakeDomainQueryName(DomainName),
                    QueryName,
                    UniqueIdentifier,
                    notificationDispatcherName: _queryDispatcherName),
                    context: _queryContext);

            if (esQry != null)
            {
                await esQry.DeleteStream();
            }
        }

        /// <summary>
        /// Create a new query instance from the parameter attribute
        /// </summary>
        /// <param name="attribute">
        /// The attribute to use to new up the query instance
        /// </param>
        public Query(QueryAttribute attribute)
            : this(attribute.DomainName ,
                  attribute.QueryName ,
                  attribute.UniqueIdentifier )
        {
        }

        public Query(string domainName,
            string queryName,
            string queryUniqueIdentifier,
            WriteContext context = null )
        {
            _domainName = domainName;
            _queryName = queryName;
            _uniqueIdentifier = queryUniqueIdentifier;
            // Make a query 
            if (context == null)
            {
                _queryContext = new WriteContext()
                {
                    Source = _queryName,
                    CausationIdentifier = _uniqueIdentifier,
                    Commentary = _queryName
                };
            }
            else
            {
                _queryContext = context;
            }
        }

        /// <summary>
        /// Make a "queries" domain for the given top level domain
        /// </summary>
        /// <param name="domainName">
        /// The top level (business) domain
        /// </param>
        /// <remarks>
        /// This allows different domains' query names to be unique even if
        /// the base query names are not
        /// </remarks>
        public static string MakeDomainQueryName(string domainName)
        {
            if (!string.IsNullOrWhiteSpace(domainName))
            {
                return domainName.Trim() + @"_Query";
            }
            else
            {
                return "Query";
            }
        }
    }
}
