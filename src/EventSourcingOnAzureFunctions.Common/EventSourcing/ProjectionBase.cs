using EventSourcingOnAzureFunctions.Common.EventSourcing.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;

namespace EventSourcingOnAzureFunctions.Common.EventSourcing
{
    /// <summary>
    /// The base class for any projection
    /// </summary>
    public abstract class ProjectionBase
        : IProjection
    {

        

        private int _sequenceNumber = 0;
        /// <summary>
        /// The current sequence number of the last event processed by this projection
        /// </summary>
        public int CurrentSequenceNumber { get { return _sequenceNumber;  } }


        public void HandleEvent(string eventTypeName, object eventToHandle)
        {
            if (typedEventHandlers.ContainsKey(eventTypeName ))
            {
                var eventPayloadAsJson = eventToHandle as Newtonsoft.Json.Linq.JObject;
                object[] invocationParameters = null;
                if (null != eventPayloadAsJson )
                {
                     invocationParameters = new object[]  { eventPayloadAsJson.ToObject(typedEventHandlers[eventTypeName].Item1 ) }; 
                }
                else
                {
                    invocationParameters = new object[] { eventToHandle };
                }
                typedEventHandlers[eventTypeName].Item2.Invoke(this, invocationParameters);
            }
        }

        public  bool HandlesEventType(string eventTypeName)
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
            // Process the as-of date if it is set
            if (asOfDate.HasValue )
            {
                _currentAsOfDate = asOfDate.Value;
            }
        }

        private string MakePropertyName(string propertyName, int rowNumber)
        {
            if ((rowNumber == ProjectionSnapshotProperty.NO_ROW_NUMBER))
                return propertyName;
            else
                return propertyName + "(" + rowNumber.ToString() + ")";
        }

        // Snapshots not implemented yet
        public bool SupportsSnapshots => false;

        // As of date not implemented yet
        private DateTime _currentAsOfDate;
        public DateTime CurrentAsOfDate
        {
            get
            {
                return _currentAsOfDate;
            }
        }


        private Dictionary<string, Tuple<Type, System.Reflection.MethodInfo>> typedEventHandlers = new Dictionary<string, Tuple<Type, System.Reflection.MethodInfo>>();

        public ProjectionBase()
        {
            // Wire up any explicit event handlers by dictionary
            foreach (var interfaceImplementation in GetType().GetInterfaces() )
            {
                if (interfaceImplementation.Name.StartsWith("IHandleEventType") )
                {
                    Type eventArgumentType = interfaceImplementation.GetGenericArguments()[0];
                    string eventName = EventNameAttribute.GetEventName(eventArgumentType);
                    System.Reflection.MethodInfo miEvent= interfaceImplementation.GetMethods()[0]; 
                    // add these to our internal dicttionary
                    if (!typedEventHandlers.ContainsKey(eventName ) )
                    {
                        typedEventHandlers.Add(eventName , new Tuple<Type, System.Reflection.MethodInfo> ( eventArgumentType , miEvent ) );
                    }
                }
            }
        }
    }
}
