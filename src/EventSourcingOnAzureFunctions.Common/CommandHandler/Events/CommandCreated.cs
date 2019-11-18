using EventSourcingOnAzureFunctions.Common.EventSourcing;
using System;

namespace EventSourcingOnAzureFunctions.Common.CommandHandler.Events
{
    /// <summary>
    /// A new command instance was created
    /// </summary>
    /// <remarks>
    /// This provides additional information above and beyond what is available in the command event 
    /// stream created notification
    /// </remarks>
    [EventName("Command Created")]
    public class Created
    {
        /// <summary>
        /// The date/time the new command was logged by the system
        /// </summary>
        public DateTime DateLogged { get; set; }

        /// <summary>
        /// If the system that initiated this command has its own way of identifying command instances
        /// this will be recorded here
        /// </summary>
        public string ExternalSystemUniqueIdentifier { get; set; }

        /// <summary>
        /// For commands that rely on authorisation this is the token passed in to test
        /// for the authorisation process
        /// </summary>
        public string AuthorisationToken { get; set; }

    }
}
