using EventSourcingOnAzureFunctions.Common.Binding;
using EventSourcingOnAzureFunctions.Common.ClassifierHandler.Events;
using EventSourcingOnAzureFunctions.Common.CQRS.ClassifierHandler.Events;
using EventSourcingOnAzureFunctions.Common.CQRS.CommandHandler;
using EventSourcingOnAzureFunctions.Common.CQRS.CommandHandler.Events;
using EventSourcingOnAzureFunctions.Common.CQRS.CommandHandler.Projections;
using EventSourcingOnAzureFunctions.Common.CQRS.Common.Events;
using EventSourcingOnAzureFunctions.Common.CQRS.ProjectionHandler.Events;
using EventSourcingOnAzureFunctions.Common.EventSourcing;
using EventSourcingOnAzureFunctions.Common.EventSourcing.Implementation;
using EventSourcingOnAzureFunctions.Common.EventSourcing.Interfaces;
using EventSourcingOnAzureFunctions.Common.Notification;
using Microsoft.Azure.EventGrid.Models;
using System;
using System.Collections.Generic;
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

        private readonly IWriteContext _commandContext;

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


        // Defined methods...
        // 1 - Set parameter
        public async Task SetParameter(string parameterName, object parameterValue)
        {
            if (!string.IsNullOrWhiteSpace(parameterName))
            {

                EventStream esCmd = new EventStream(new EventStreamAttribute(MakeDomainCommandName( DomainName ),
                    CommandName,
                    UniqueIdentifier),
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
                    nameof(CommandHandler.Projections.ParameterValues)));
               
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
                nameof(CommandHandler.Projections.ExecutionState)));

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
        /// Initiate a command step
        /// </summary>
        /// <param name="stepName">
        /// The name of the step to run
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
                    UniqueIdentifier),
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
                    UniqueIdentifier),
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


        //Classifier request
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
                UniqueIdentifier),
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

            EventStream esCmd = new EventStream(new EventStreamAttribute(MakeDomainCommandName(DomainName),
                CommandName,
                UniqueIdentifier),
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
                        Nullable<DateTime> asOfDate)
        {
            Guid correlationId = Guid.NewGuid();

            EventStream esCmd = new EventStream(new EventStreamAttribute(
                       MakeDomainCommandName(DomainName),
                       CommandName,
                       UniqueIdentifier),
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
        }

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
                UniqueIdentifier),
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


        public Command (string domainName,
            string commandName,
            string uniqueIdentifier)
            : this(new CommandAttribute(domainName, commandName, uniqueIdentifier ) )
        {
        }

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
            }
        }

        public Command(EventGridEvent egCommandNotification)
        {
            if (null != egCommandNotification )
            {
                // get the NewEventEventGridPayload from the event grid event
                NewEventEventGridPayload payload = egCommandNotification.Data as NewEventEventGridPayload;
                if (null != payload )
                {
                    _domainName = payload.DomainName;
                    _commandName = payload.EntityTypeName;
                    _uniqueIdentifier = payload.InstanceKey;
                }
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
