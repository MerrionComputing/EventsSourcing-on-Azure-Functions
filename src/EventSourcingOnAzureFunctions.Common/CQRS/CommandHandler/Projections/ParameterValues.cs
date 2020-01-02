using EventSourcingOnAzureFunctions.Common.CQRS.Common.Events;
using EventSourcingOnAzureFunctions.Common.EventSourcing;
using EventSourcingOnAzureFunctions.Common.EventSourcing.Interfaces;
using System;
using System.Collections.Generic;

namespace EventSourcingOnAzureFunctions.Common.CQRS.CommandHandler.Projections
{
    /// <summary>
    /// A projection to get the current set of parameters and their values 
    /// for any given command
    /// </summary>
    public sealed class ParameterValues
        : ProjectionBase,
        IHandleEventType<Common.Events.ParameterValueSet >   
    {

        private Dictionary<string, object> _parameterValues = new Dictionary<string, object>();

        public IReadOnlyDictionary<string,object> Values
        {
            get
            {
                return _parameterValues;
            }
        }

        public void HandleEventInstance(ParameterValueSet eventInstance)
        {
            if (null != eventInstance )
            {
                if (_parameterValues.ContainsKey(eventInstance.Name ) )
                {
                    _parameterValues[eventInstance.Name] = eventInstance.Value; 
                }
                else
                {
                    _parameterValues.Add(eventInstance.Name, eventInstance.Value);
                }
            }
        }
    }
}
