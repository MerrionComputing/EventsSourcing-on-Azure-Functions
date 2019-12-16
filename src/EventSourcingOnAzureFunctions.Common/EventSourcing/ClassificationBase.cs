using EventSourcingOnAzureFunctions.Common.EventSourcing.Interfaces;
using System;
using System.Collections.Generic;

namespace EventSourcingOnAzureFunctions.Common.EventSourcing
{
    public abstract class ClassificationBase
        : IClassification 
    {
        private int _sequenceNumber = 0;
        /// <summary>
        /// The current sequence number of the last event processed by this projection
        /// </summary>
        public int CurrentSequenceNumber { get { return _sequenceNumber; } }


        public bool HandlesEventType(string eventTypeName)
        {
            if (!string.IsNullOrWhiteSpace(eventTypeName))
            {
                if (typedEventHandlers.ContainsKey(eventTypeName))
                {
                    return true;
                }
            }
            return false;
        }


        private int _lastHandledSequenceNumber;
        public void MarkEventHandled(int handledEventSequenceNumber)
        {
            _lastHandledSequenceNumber = handledEventSequenceNumber;
        }

        public void OnEventRead(int sequenceNumber, DateTime? asOfDate)
        {
            _sequenceNumber = sequenceNumber;
            // TODO : Process the as-of date if it is set
        }

        public ClassificationResponse.ClassificationResults HandleEvent(string eventTypeName, object eventToHandle)
        {
            if (typedEventHandlers.ContainsKey(eventTypeName))
            {
                var eventPayloadAsJson = eventToHandle as Newtonsoft.Json.Linq.JObject;
                object[] invocationParameters = null;
                if (null != eventPayloadAsJson)
                {
                    invocationParameters = new object[] { eventPayloadAsJson.ToObject(typedEventHandlers[eventTypeName].Item1) };
                }
                else
                {
                    invocationParameters = new object[] { eventToHandle };
                }
                return (ClassificationResponse.ClassificationResults)typedEventHandlers[eventTypeName].Item2.Invoke(this, invocationParameters);
            }
            return ClassificationResponse.ClassificationResults.Unchanged;
        }

        // Snapshots not implemented yet
        public bool SupportsSnapshots => false;

        // As of date not implemented yet
        public DateTime CurrentAsOfDate => throw new NotImplementedException();

        /// <summary>
        /// Allow parameters to be passed to the classification class
        /// </summary>
        /// <param name="parameterName">
        /// The unique name of the parameter
        /// </param>
        /// <param name="parameterValue">
        /// The value to use for that parameter
        /// </param>
        public abstract void SetParameter(string parameterName, object parameterValue);

        private Dictionary<string, Tuple<Type, System.Reflection.MethodInfo>> typedEventHandlers = new Dictionary<string, Tuple<Type, System.Reflection.MethodInfo>>();


        public ClassificationBase()
        {
            // Wire up any explicit event handlers by dictionary
            foreach (var interfaceImplementation in GetType().GetInterfaces())
            {
                if (interfaceImplementation.Name.StartsWith("IClassifyEventType"))
                {
                    Type eventArgumentType = interfaceImplementation.GetGenericArguments()[0];
                    string eventName = EventNameAttribute.GetEventName(eventArgumentType);
                    System.Reflection.MethodInfo miEvent = interfaceImplementation.GetMethods()[0];
                    // add these to our internal dicttionary
                    if (!typedEventHandlers.ContainsKey(eventName))
                    {
                        typedEventHandlers.Add(eventName, new Tuple<Type, System.Reflection.MethodInfo>(eventArgumentType, miEvent));
                    }
                }
            }
        }
    }
}
