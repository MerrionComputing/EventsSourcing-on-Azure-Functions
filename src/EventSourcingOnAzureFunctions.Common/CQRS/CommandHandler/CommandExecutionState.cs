using System;
using System.Collections.Generic;
using System.Text;

namespace EventSourcingOnAzureFunctions.Common.CQRS.CommandHandler
{
    /// <summary>
    /// The current state of a command execution
    /// </summary>
    public class CommandExecutionState
    {

        /// <summary>
        /// The current status of the command 
        /// </summary>
        public string CurrentStatus { get; set; }

        /// <summary>
        /// Additional message of the command execution state
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// The sequence number as-of which thsi status was in force
        /// </summary>
        public int AsOfSequenceNumber { get; set; }
    }
}
