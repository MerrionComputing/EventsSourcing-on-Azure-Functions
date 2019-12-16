using EventSourcingOnAzureFunctions.Common.Binding;
using System;
using System.Collections.Generic;
using System.Text;

namespace EventSourcingOnAzureFunctions.Common.CQRS
{
    /// <summary>
    /// A wrapper class used to trigger and manage a query orchestration
    /// </summary>
    public class Command
    {


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


        public Command(CommandAttribute attribute)
        {
            if (null != attribute )
            {
                _commandName = attribute.CommandName;
                _uniqueIdentifier = attribute.UniqueIdentifier;
            }
        }
    }
}
