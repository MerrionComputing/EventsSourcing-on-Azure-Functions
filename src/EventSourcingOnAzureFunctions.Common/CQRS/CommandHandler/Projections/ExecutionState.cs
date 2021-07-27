using EventSourcingOnAzureFunctions.Common.CQRS.CommandHandler.Events;
using EventSourcingOnAzureFunctions.Common.EventSourcing;
using EventSourcingOnAzureFunctions.Common.EventSourcing.Interfaces;
using System;
using System.Collections.Generic;

namespace EventSourcingOnAzureFunctions.Common.CQRS.CommandHandler.Projections
{
    /// <summary>
    /// A projection to get the execution state (Running, Error, Completed) 
    /// for a given command instance
    /// </summary>
    [ProjectionName("Command Execution State") ]
    public sealed class ExecutionState
        : ProjectionBase,
        IHandleEventType<Created>,
        IHandleEventType<FatalErrorOccured>,
        IHandleEventType<Completed>,
        IHandleEventType<CommandStepInitiated>,
        IHandleEventType<StepCompleted>
    {

        // String constants for the different status
        public const string STATUS_NEW = @"New";
        public const string STATUS_RUNNING = @"Running";
        public const string STATUS_ERROR = @"In Error";
        public const string STATUS_COMPLETE = @"Complete";

        private string _currentStatus = STATUS_NEW;
        /// <summary>
        /// The command status as at when the projection was executed
        /// </summary>
        public string CurrentStatus
        {
            get
            {
                return _currentStatus;
            }
        }

        private string _message = @"";
        public string Message
        {
            get
            {
                return _message;
            }
        }

        private string _currentStep = @"";
        /// <summary>
        /// The name of the step currently being executed
        /// </summary>
        public string CurrentStep
        {
            get
            {
                return _currentStep;
            }
        }

        public void HandleEventInstance(Created eventInstance)
        {
            if (null != eventInstance)
            {
                _currentStatus = STATUS_NEW;
                _message = $"Created {eventInstance.DateLogged}";
            }
        }

        public void HandleEventInstance(FatalErrorOccured eventInstance)
        {
            if (null != eventInstance)
            {
                _currentStatus = STATUS_ERROR;
                _message = eventInstance.Message;
            }
        }

        public void HandleEventInstance(Completed eventInstance)
        {
            if (null != eventInstance)
            {
                _currentStatus = STATUS_COMPLETE;
                _message = eventInstance.Notes;
            }
        }

        public void HandleEventInstance(CommandStepInitiated eventInstance)
        {
            if (null != eventInstance)
            {
                _currentStatus = STATUS_RUNNING;
                _message = $"Running {eventInstance.StepName}";
                _currentStep = eventInstance.StepName;
            }
        }

        public void HandleEventInstance(StepCompleted eventInstance)
        {
            if (null != eventInstance)
            {
                _currentStatus = STATUS_RUNNING;
                _message = $"Completed {eventInstance.StepName} - {eventInstance.Message} ";
            }
        }
    }
}
