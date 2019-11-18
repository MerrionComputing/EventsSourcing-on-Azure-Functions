using EventSourcingOnAzureFunctions.Common.EventSourcing;
using System;


namespace EventSourcingOnAzureFunctions.Common.CommandHandler.Events
{

    /// <summary>
    /// A fatal error has occured when processing this command
    /// </summary>
    /// <remarks>
    /// If there are rollbacks or compensating events to be performed 
    /// </remarks>
    [EventName("Fatal Error Occured")]
    public class FatalErrorOccured
    {

        /// <summary>
        /// The date/time the error was logged by the system
        /// </summary>
        public DateTime DateLogged { get; set; }

        /// <summary>
        /// The text message explaining what the error was
        /// </summary>
        public string Message { get; set; }
    }
}
