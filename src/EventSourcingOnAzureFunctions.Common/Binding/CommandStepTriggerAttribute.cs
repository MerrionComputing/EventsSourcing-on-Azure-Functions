using System;
using EventSourcingOnAzureFunctions.Common.EventSourcing.Interfaces;
using Microsoft.Azure.WebJobs.Description;

namespace EventSourcingOnAzureFunctions.Common.Binding
{

    /// <summary>
    /// Attribute to mark a function to be triggered for a given command step
    /// </summary>
    [AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false)]
    [Binding]
    public sealed class CommandStepTriggerAttribute
        : Attribute
    {

        private readonly string _domainName;
        /// <summary>
        /// The domain in which the command is being executed monitored is located
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
        /// The name of the command for which this step is a part
        /// </summary>
        public string CommandName
        {
            get 
            {
                return _commandName; 
            }
        }

       
        private readonly string _commandStepName;
        /// <summary>
        /// The name of the specific step of this command
        /// </summary>
        public string CommandStepName
        {
            get
            {
                return _commandStepName;
            }
        }

        /// <summary>
        /// Mark a function to be triggered for a given command step
        /// </summary>
        /// <param name="DomainName">
        /// The name of the business domain this command is executed in
        /// </param>
        /// <param name="CommandName">
        /// The name of the command to execute
        /// </param>
        /// <param name="StepName">
        /// The name of the step to execute
        /// </param>
        public CommandStepTriggerAttribute(string DomainName,
            string CommandName,
            string StepName)
        {
            _domainName = DomainName;
            _commandName = CommandName;
            _commandStepName = StepName;
        }
    }
}
