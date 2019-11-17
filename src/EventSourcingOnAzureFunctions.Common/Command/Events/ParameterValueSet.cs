using EventSourcingOnAzureFunctions.Common.EventSourcing;
using System;


namespace EventSourcingOnAzureFunctions.Common.Command.Events
{
    /// <summary>
    /// A parameter to be used when executing the command has been set
    /// </summary>
    /// <remarks>
    /// Each parameter gets its own event so that they can be overwritten during the 
    /// processing of the command if needed
    /// </remarks>
    [EventName("Command Parameter Value Set")]
    public class ParameterValueSet
    {


        /// <summary>
        /// The name of the parameter
        /// </summary>
        /// <remarks>
        /// This needs to be unique within the context of this command
        /// </remarks>
        public string Name { get; set; }


        /// <summary>
        /// The value for the parameter
        /// </summary>
        public object Value { get; set; }

    }
}
