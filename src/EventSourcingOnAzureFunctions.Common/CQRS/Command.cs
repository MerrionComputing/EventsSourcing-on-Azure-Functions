using Azure.Messaging.EventGrid;
using EventSourcingOnAzureFunctions.Common.Binding;
using EventSourcingOnAzureFunctions.Common.ClassifierHandler.Projections;
using EventSourcingOnAzureFunctions.Common.CQRS.ClassifierHandler.Events;
using EventSourcingOnAzureFunctions.Common.CQRS.CommandHandler;
using EventSourcingOnAzureFunctions.Common.CQRS.CommandHandler.Events;
using EventSourcingOnAzureFunctions.Common.CQRS.CommandHandler.Projections;
using EventSourcingOnAzureFunctions.Common.CQRS.Common.Events;
using EventSourcingOnAzureFunctions.Common.CQRS.ProjectionHandler.Events;
using EventSourcingOnAzureFunctions.Common.CQRS.ProjectionHandler.Projections;
using EventSourcingOnAzureFunctions.Common.EventSourcing;
using EventSourcingOnAzureFunctions.Common.EventSourcing.Implementation;
using EventSourcingOnAzureFunctions.Common.EventSourcing.Interfaces;
using EventSourcingOnAzureFunctions.Common.Notification;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
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
    public class Command
    {

        private readonly string _commandDispatcherName = nameof(QueueNotificationDispatcher);
        private readonly IWriteContext _commandContext;
        private Common.Listener.CommandListener _commandListener;

        private readonly string _domainName;
        /// <summary>
        /// The business domain in which the command is located
        /// </summary>
        public string DomainName
        {
            get
            {
                return _domainName;
            }
        }

        private readonly string _commandName;
        /// <summary>
        /// The name of the command to run
        /// </summary>
        public string CommandName
        {
            get
            {
                return _commandName;
            }
        }

        private readonly string _uniqueIdentifier;
        /// <summary>
        /// The unique instance of the command to run
        /// </summary>
        public string UniqueIdentifier
        {
            get
            {
                return _uniqueIdentifier;
            }
        }

        /// <summary>
        /// Get the command attribute that is used to define this command instance
        /// </summary>
        public CommandAttribute AsAttribute()
        {
            return new CommandAttribute(_domainName, _commandName, _uniqueIdentifier);
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

            EventStream esCmd = new EventStream(new EventStreamAttribute(MakeDomainCommandName(DomainName),
                                CommandName,
                                UniqueIdentifier,
                                notificationDispatcherName: _commandDispatcherName),
                                context: _commandContext);

            Created evCreated = new Created()
            {
                AuthorisationToken = AuthorisationToken,
                ExternalOrchestrationIdentifier = ExternalOrchestrationIdentifier,
                ExternalSystemUniqueIdentifier = ExternalSystemUniqueIdentifier,
                DateLogged = DateTime.UtcNow
            };

            await esCmd.AppendEvent(evCreated);

            // Start the listener 
            if (_commandListener != null)
            {
                // TODO : Start a listener / executor for this command
            }
        }


        /// <summary>
        /// Set a parameter value to be used in executing the command
        /// </summary>
        /// <param name="parameterName">
        /// The name of the parameter
        /// </param>
        /// <param name="parameterValue">
        /// The value to use for that named parameter
        /// </param>
        public async Task SetParameter(string parameterName, object parameterValue)
        {
            if (!string.IsNullOrWhiteSpace(parameterName))
            {

                EventStream esCmd = new EventStream(new EventStreamAttribute(MakeDomainCommandName( DomainName ),
                    CommandName,
                    UniqueIdentifier,
                    notificationDispatcherName: _commandDispatcherName),
                    context: _commandContext);

                ParameterValueSet evParam = new ParameterValueSet()
                { 
                    Name = parameterName,
                    Value = parameterValue
                };

                await esCmd.AppendEvent(evParam);
            }
        }

        /// <summary>
        /// Get the current value set for a parameter
        /// </summary>
        /// <param name="parameterName">
        /// The name of the parameter to get
        /// </param>
        /// <param name="asOfDate">
        /// (Optional) The as-of date for which to get the parameter value
        /// </param>
        /// <returns>
        /// If no parameter with the given name exists this will return null
        /// </returns>
        public async Task<object> GetParameterValue(string parameterName,
            Nullable<DateTime> asOfDate = null)
        {
            if (!string.IsNullOrWhiteSpace(parameterName))
            {
                Projection prjCmdParams = new Projection(
                    new ProjectionAttribute(MakeDomainCommandName(DomainName),
                    CommandName,
                    UniqueIdentifier,
                    nameof(CommandHandler.Projections.ParameterValues),
                    notificationDispatcherName: _commandDispatcherName));
               
                if (null != prjCmdParams)
                {
                    ParameterValues values = await prjCmdParams.Process<ParameterValues>(asOfDate);
                    if (null != values )
                    {
                        if (values.Values.ContainsKey(parameterName )  )
                        {
                            return values.Values[parameterName]; 
                        }
                    }
                }
            }

            // If no parameter is found, default to null
            return null;
        }

        /// <summary>
        /// Get the current values set of the parameters of this command
        /// </summary>
        /// <param name="asOfDate">
        /// (Optional) The as-of date for which to get the parameter values
        /// </param>
        /// <returns></returns>
        public async Task<IReadOnlyDictionary<string,object>> GetParameterValues(Nullable<DateTime> asOfDate = null)
        {
            Projection prjCmdParams = new Projection(
                new ProjectionAttribute(MakeDomainCommandName(DomainName),
                CommandName,
                UniqueIdentifier,
                nameof(CommandHandler.Projections.ParameterValues)));

            if (null != prjCmdParams)
            {
                ParameterValues values = await prjCmdParams.Process<ParameterValues>(asOfDate);
                if (null != values)
                {
                    if (null != values.Values)
                    {
                        return values.Values;
                    }
                }
            }
            // If no parameters found default to an empty list
            return new Dictionary<string, object>(); 
        }


        /// <summary>
        /// Get the current execution state of this command
        /// </summary>
        /// <remarks>
        /// This can be used to decide whether to retry a failed command or 
        /// to perform post-completion processing etc.
        /// </remarks>
        public async Task<CommandExecutionState> GetExecutionState()
        {
            Projection prjCmdState = new Projection(
                new ProjectionAttribute(MakeDomainCommandName(DomainName),
                CommandName,
                UniqueIdentifier,
                nameof(CommandHandler.Projections.ExecutionState),
                notificationDispatcherName:_commandDispatcherName ));

            if (null != prjCmdState)
            {
                var stateProjection=  await prjCmdState.Process<ExecutionState>();  
                if (null != stateProjection )
                {
                    return new CommandExecutionState()
                    {
                        CurrentStatus = stateProjection.CurrentStatus,
                        AsOfSequenceNumber = stateProjection.CurrentSequenceNumber,
                        Message = stateProjection.Message
                    };
                }
            }

            // State cannot be retrieved
            return null;
        }

        /// <summary>
        /// Cancel this command 
        /// </summary>
        /// <param name="Notes">
        /// Notes to add to the command to describe why/how it was cancelled
        /// </param>
        /// <param name="CompensationInitiated">
        /// Whether to kick off compensating actions for this command to undo what it has
        /// done so far
        /// </param>
        public async Task Cancel(string Notes = "",
            bool CompensationInitiated = false )
        {

            EventStream esCmd = new EventStream(new EventStreamAttribute(MakeDomainCommandName(DomainName),
                    CommandName,
                    UniqueIdentifier,
                    notificationDispatcherName: _commandDispatcherName),
                    context: _commandContext);

            CommandCancelled evCancel = new CommandCancelled()
            {
                Notes = Notes,
                CompensationInitiated = CompensationInitiated,
                DateCancelled = DateTime.UtcNow
            };

            await esCmd.AppendEvent(esCmd);

            
            if (_commandListener != null)
            {
                // TODO : Perform any cancellation action(s) by listener / activator
            }
        }

        /// <summary>
        /// Initiate a command step
        /// </summary>
        /// <param name="stepName">
        /// The name of the step to run
        /// </param>
        /// <param name="targetDomain">
        /// The domain of the entity on which the command is to be executed
        /// </param>
        /// <param name="targetEntityType">
        /// The type of the entity on which the command isw to be executed
        /// </param>
        /// <param name="targetEntityInstance">
        /// The unique instance of the entity on which the command is to be executed
        /// </param>
        public async Task InitiateStep(string stepName,
            string targetDomain = "",
            string targetEntityType = "",
            string targetEntityInstance = "")
        {
            if (!string.IsNullOrWhiteSpace(stepName))
            {

                EventStream esCmd = new EventStream(new EventStreamAttribute(MakeDomainCommandName(DomainName),
                    CommandName,
                    UniqueIdentifier,
                    notificationDispatcherName: _commandDispatcherName),
                    context: _commandContext);

                CommandStepInitiated evStep = new CommandStepInitiated()
                {
                    StepName = stepName,
                    DomainName = targetDomain ,
                    EntityTypeName = targetEntityType ,
                    InstanceKey = targetEntityInstance 
                };

                await esCmd.AppendEvent(evStep);
            }

            if (_commandListener != null)
            {
                // TODO : Use the listener / executor to run the command step
            }
        }

        /// <summary>
        /// Mark a command step as being completed
        /// </summary>
        /// <param name="stepName">
        /// The name of the step that has been completed
        /// </param>
        /// <param name="completionMessage">
        /// Additional text for the step completion
        /// </param>
        /// <param name="targetDomain">
        /// The domain of the entity on which the command was executed
        /// </param>
        /// <param name="targetEntityType">
        /// The type of the entity on which the command was executed
        /// </param>
        /// <param name="targetEntityInstance">
        /// The unique instance of the entity on which the command was executed
        /// </param>
        public async Task StepCompleted(string stepName,
            string completionMessage,
            string targetDomain = "",
            string targetEntityType = "",
            string targetEntityInstance = "")
        {
            if (!string.IsNullOrWhiteSpace(stepName))
            {

                EventStream esCmd = new EventStream(new EventStreamAttribute(MakeDomainCommandName(DomainName),
                    CommandName,
                    UniqueIdentifier,
                    notificationDispatcherName: _commandDispatcherName),
                    context: _commandContext);

                StepCompleted evStep = new StepCompleted()
                {
                    StepName = stepName,
                    Message = completionMessage ,
                    DateLogged = DateTime.UtcNow ,
                    DomainName = targetDomain,
                    EntityTypeName = targetEntityType,
                    InstanceKey = targetEntityInstance
                };

                await esCmd.AppendEvent(evStep);
            }
        }


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
                        Nullable<DateTime> asOfDate,
                        IEnumerable<KeyValuePair<string, object>> classificationParameters)
        {

            // Correlation to link the parameters to the classification 
            Guid correlationId = Guid.NewGuid();

            EventStream esCmd = new EventStream(new EventStreamAttribute(MakeDomainCommandName(DomainName),
                CommandName,
                UniqueIdentifier,
                notificationDispatcherName: _commandDispatcherName),
                context: _commandContext);

            if (null != classificationParameters)
            {
                // add a classification request parameter for each...
                foreach (KeyValuePair<string, object> parameter in classificationParameters)
                {
                    ClassifierRequestParameterSet evParam = new ClassifierRequestParameterSet()
                    {
                        CorrelationIdentifier = correlationId.ToString(),
                        ParameterName = parameter.Key,
                        ParameterValue = parameter.Value
                    };

                    await esCmd.AppendEvent(evParam);
                }
            }

            // add the classification request
            ClassifierRequested evRequest = new ClassifierRequested()
            {
                CorrelationIdentifier = correlationId.ToString(),
                DomainName = domainName,
                EntityTypeName = entityTypeName,
                InstanceKey = instanceKey,
                ClassifierTypeName = classifierTypeName,
                AsOfDate = asOfDate
            };

            await esCmd.AppendEvent(evRequest);

            if (_commandListener != null)
            {
                // TODO : Use the listener / executor to run the classification
            }
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
        public async Task PostClassifierResponse(string domainName,
                        string entityTypeName,
                        string instanceKey,
                        string classifierTypeName,
                        Nullable<DateTime> asOfDate,
                        string correlationIdentifier,
                        int asOfSequenceNumber,
                        ClassificationResults result)
        {

            EventStream esCmd = new EventStream(new EventStreamAttribute(MakeDomainCommandName(DomainName),
                CommandName,
                UniqueIdentifier,
                notificationDispatcherName: _commandDispatcherName),
                context: _commandContext);

            ClassifierResultReturned evRet = new ClassifierResultReturned()
            {
                DomainName = domainName,
                EntityTypeName = entityTypeName,
                InstanceKey = instanceKey,
                ClassifierTypeName = classifierTypeName,
                AsOfDate = asOfDate,
                AsOfSequenceNumber = asOfSequenceNumber,
                CorrelationIdentifier = correlationIdentifier,
                Result = result
            };


            await esCmd.AppendEvent(evRet);
        }

        /// <summary>
        /// Gets the set of projection requests outstanding for this query 
        /// </summary>
        /// <returns>
        /// The set of projection requests with no matching response
        /// </returns>
        public async Task<IEnumerable<IClassifierRequest>> GetOutstandingClassifiers()
        {
            // Run the [Outstanding Classifications] projection over this command's event stream.
            Projection outstanding = new Projection(new ProjectionAttribute(MakeDomainCommandName(DomainName),
                CommandName,
                UniqueIdentifier,
                ProjectionNameAttribute.GetProjectionName(typeof(OutstandingClassifications)),
                notificationDispatcherName: _commandDispatcherName)
                );

            var outstandingClassifications = await outstanding.Process<OutstandingClassifications>();

            if (outstandingClassifications != null)
            {
                return outstandingClassifications.ClassificationsToBeProcessed;
            }

            // If nothing found return an empty set for composability
            return Enumerable.Empty<IClassifierRequest>();
        }

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
        /// <param name="projectionTypeName">
        /// The specific type of projection process to run over the event stream
        /// </param>
        /// <param name="asOfDate">
        /// (Optional) The date up to which to run the classification
        /// </param>
        public async Task RequestProjection(string domainName,
                        string entityTypeName,
                        string instanceKey,
                        string projectionTypeName,
                        Nullable<DateTime> asOfDate)
        {
            Guid correlationId = Guid.NewGuid();

            EventStream esCmd = new EventStream(new EventStreamAttribute(
                       MakeDomainCommandName(DomainName),
                       CommandName,
                       UniqueIdentifier,
                       notificationDispatcherName: _commandDispatcherName),
                       context: _commandContext);

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

            await esCmd.AppendEvent(evPrj);

            if (_commandListener != null)
            {
                // TODO : Use the listener / executor to run the projection
            }
        }

        /// <summary>
        /// Append a projection response to the command event stream
        /// </summary>
        /// <param name="domainName"></param>
        /// <param name="entityTypeName"></param>
        /// <param name="instanceKey"></param>
        /// <param name="projectionTypeName"></param>
        /// <param name="asOfDate"></param>
        /// <param name="correlationIdentifier"></param>
        /// <param name="asOfSequenceNumber"></param>
        /// <param name="projectionResult"></param>
        /// <returns></returns>
        public async Task PostProjectionResponse(string domainName,
                        string entityTypeName,
                        string instanceKey,
                        string projectionTypeName,
                        Nullable<DateTime> asOfDate,
                        string correlationIdentifier,
                        int asOfSequenceNumber,
                        IProjection projectionResult)
        {

            EventStream esCmd = new EventStream(new EventStreamAttribute(MakeDomainCommandName(DomainName),
                CommandName,
                UniqueIdentifier,
                notificationDispatcherName: _commandDispatcherName),
                context: _commandContext);


            ProjectionValueReturned evRet = new ProjectionValueReturned()
            {
                ProjectionDomainName = domainName,
                ProjectionEntityTypeName = entityTypeName,
                ProjectionInstanceKey = instanceKey,
                AsOfDate = asOfDate,
                AsOfSequenceNumber = asOfSequenceNumber,
                CorrelationIdentifier = correlationIdentifier,
                ProjectionTypeName = projectionTypeName,
                DateLogged = DateTime.UtcNow,
                Value = projectionResult.ToString()
            };

            await esCmd.AppendEvent(evRet);
        }

        /// <summary>
        /// Gets the set of projection requests outstanding for this command 
        /// </summary>
        /// <returns>
        /// The set of projection requests with no matching response
        /// </returns>
        public async Task<IEnumerable<IProjectionRequest>> GetOutstandingProjections()
        {

            // Run the [Outstanding Projections] projection over this command's event stream.
            Projection outstanding = new Projection(new ProjectionAttribute(MakeDomainCommandName(DomainName),
                CommandName,
                UniqueIdentifier,
                ProjectionNameAttribute.GetProjectionName(typeof(OutstandingProjections)),
                notificationDispatcherName: _commandDispatcherName )
                );

            var outstandingProjections = await outstanding.Process<OutstandingProjections>();

            if (outstandingProjections != null)
            {
                return outstandingProjections.ProjectionsToBeProcessed;
            }

            // If nothing found return an empty set for composability
            return Enumerable.Empty<IProjectionRequest>();
        }

        /// <summary>
        /// Delete the command backing event stream
        /// </summary>
        public async Task Delete()
        {
            EventStream esCmd = new EventStream(new EventStreamAttribute(MakeDomainCommandName(DomainName),
                CommandName,
                UniqueIdentifier,
                notificationDispatcherName: _commandDispatcherName),
                context: _commandContext);

            if (esCmd != null)
            {
                await esCmd.DeleteStream();
            }
        }

        /// <summary>
        /// Start the command listener / executor linked to this command
        /// </summary>
        private void StartListener()
        {
            if (_commandListener == null)
            {
                _commandListener = new Common.Listener.CommandListener(_domainName, _commandName, _uniqueIdentifier);

            }
        }

        /// <summary>
        /// Create a command tied to a specific durable functions orchestration
        /// </summary>
        /// <param name="domainName">
        /// The domain in which the command is being run
        /// </param>
        /// <param name="commandName">
        /// The name of the type of command 
        /// </param>
        /// <param name="uniqueIdentifier">
        /// The unique identifier for this instance of the command
        /// </param>
        public Command (string domainName,
            string commandName,
            string uniqueIdentifier)
            : this(new CommandAttribute(domainName, commandName, uniqueIdentifier ) )
        {
        }

        /// <summary>
        /// Create a new command from the properties of the binding attribute
        /// </summary>
        /// <param name="attribute">
        /// The attribute defining the domain, command name and instance identifier of the command instance
        /// </param>
        public Command(CommandAttribute attribute)
        {
            if (null != attribute )
            {
                _domainName = attribute.DomainName;
                _commandName = attribute.CommandName;
                _uniqueIdentifier = attribute.UniqueIdentifier;
                // Make a command context
                _commandContext = new WriteContext()
                {
                    Source = _commandName,
                    CausationIdentifier = _uniqueIdentifier 
                };

                StartListener();
            }
        }

        /// <summary>
        /// Create a command tied to a specific durable functions orchestration
        /// </summary>
        /// <param name="domainName">
        /// The domain in which the command is being run
        /// </param>
        /// <param name="commandName">
        /// The name of the type of command 
        /// </param>
        /// <param name="durableOrchestrationContext">
        /// The durable functions orchestration that this command instance is tied to
        /// </param>
        /// <remarks>
        /// Only one command instance can be connected to a given durable functions orchestration instance
        /// </remarks>
        public Command(string domainName,
            string commandName,
            IDurableOrchestrationContext durableOrchestrationContext )
            : this( domainName, commandName , durableOrchestrationContext.InstanceId )
        {
        }

        /// <summary>
        /// Create a new command instance from an event grid notification
        /// </summary>
        /// <param name="egCommandNotification">
        /// The notification that triggered the creation of this command instance
        /// </param>
        public Command(EventGridEvent egCommandNotification)
        {
            if (null != egCommandNotification )
            {
                // get the NewEventEventGridPayload from the event grid event
                NewEventEventGridPayload payload = egCommandNotification.Data.ToObjectFromJson<NewEventEventGridPayload>();
                if (null != payload )
                {
                    _domainName = payload.DomainName;
                    _commandName = payload.EntityTypeName;
                    _uniqueIdentifier = payload.InstanceKey;
                }

                StartListener();
            }
        }

        /// <summary>
        /// Make a "commands" domain for the given top level domain
        /// </summary>
        /// <param name="domainName">
        /// The top level (business) domain
        /// </param>
        /// <remarks>
        /// This allows different domains' command names to be unique even if
        /// the base command names are not
        /// </remarks>
        public static string MakeDomainCommandName(string domainName)
        {
            if (!string.IsNullOrWhiteSpace(domainName))
            {
                return domainName.Trim() + @"_Command";
            }
            else
            {
                return "Command";
            }
        }
    }
}
