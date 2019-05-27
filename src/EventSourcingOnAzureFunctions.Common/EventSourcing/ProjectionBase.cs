using EventSourcingOnAzureFunctions.Common.EventSourcing.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

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


        private List<ProjectionSnapshotProperty> _currentValues = new List<ProjectionSnapshotProperty>();
        public IEnumerable<ProjectionSnapshotProperty> CurrentValues => _currentValues ;

        public void HandleEvent(string eventTypeName, object eventToHandle)
        {
            if (typedEventHandlers.ContainsKey(eventTypeName ))
            {
                var eventPayloadAsJson = eventToHandle as Newtonsoft.Json.Linq.JObject;
                if (null != eventPayloadAsJson )
                {
                    object[] invocationParameters = { eventPayloadAsJson.ToObject(typedEventHandlers[eventTypeName].Item1 ) };
                    typedEventHandlers[eventTypeName].Item2.Invoke(this, invocationParameters);
                }
            }
        }

        public  bool HandlesEventType(string eventTypeName)
        {
            if (typedEventHandlers.ContainsKey(eventTypeName))
            {
                return true;
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

        // Snapshots not implemented yet
        public bool SupportsSnapshots => false;

        // As of date not implemented yet
        public DateTime CurrentAsOfDate => throw new NotImplementedException();


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
