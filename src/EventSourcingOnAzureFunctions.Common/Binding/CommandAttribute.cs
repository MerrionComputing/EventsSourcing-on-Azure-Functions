using System;

using EventSourcingOnAzureFunctions.Common.EventSourcing.Interfaces;
using Microsoft.Azure.WebJobs.Description;

namespace EventSourcingOnAzureFunctions.Common.Binding
{
    /// <summary>
    /// An attribute to mark a parameter as being a command executor
    /// </summary>
    [AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false)]
    [Binding]
    public class CommandAttribute
        : Attribute
    {

        private readonly string _domainName;
        /// <summary>
        /// The business domain in which the command is located
        /// </summary>
        [AutoResolve]
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
        [AutoResolve]
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
        /// <remarks>
        /// If this is not set then a new GUID will be used instead
        /// </remarks>
        [AutoResolve]
        public string UniqueIdentifier
        {
            get
            {
                return _uniqueIdentifier;
            }
        }


        // Note: The parameter names need to match the property names (except for the camelCase) because 
        // the autoresolve uses this fact to perform the instantiation
        public CommandAttribute(string domainName, 
            string commandName,
            string uniqueIdentifier = @"")
        {
            _domainName = domainName;
            _commandName = commandName;
            if (string.IsNullOrWhiteSpace(uniqueIdentifier))
            {
                _uniqueIdentifier = Guid.NewGuid().ToString("D");
            }
            else
            {
                _uniqueIdentifier = uniqueIdentifier;
            }
        }
    }
}
