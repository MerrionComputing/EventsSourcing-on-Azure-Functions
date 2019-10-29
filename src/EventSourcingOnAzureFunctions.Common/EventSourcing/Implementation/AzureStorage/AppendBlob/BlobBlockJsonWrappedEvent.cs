using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using EventSourcingOnAzureFunctions.Common.EventSourcing.Interfaces;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace EventSourcingOnAzureFunctions.Common.EventSourcing.Implementation.AzureStorage.AppendBlob
{
    [JsonObject(Title = "")]
    public class BlobBlockJsonWrappedEvent
        : IJsonSerialisedEvent, IEventContext
    {

        /// <summary>
        /// The name of the type of event this is
        /// </summary>
        public string EventTypeName { get; set; }

        /// <summary>
        /// The version number of the event schema 
        /// </summary>
        public int VersionNumber { get; set; }

        /// <summary>
        /// The incremental sequence number of this event in the stream/history in which it is written
        /// </summary>
        public int SequenceNumber { get; set; }

        /// <summary>
        /// The inner event detail converted into a JSON class
        /// </summary>
        public JObject EventInstanceAsJson { get; set; }

        /// <summary>
        /// Who caused this event to be written 
        /// </summary>
        public string Who { get; set; }


        /// <summary>
        /// Where did the command to write this event come from
        /// </summary>
        public string Source { get; set; }

        /// <summary>
        /// Additional context commentary to add to the event wrapper
        /// </summary>
        /// <remarks>
        /// This is not for business meaningful data but rather for debugging or logging purposes
        /// </remarks>
        public string Commentary { get; set; }

        /// <summary>
        /// Correlation identifier used to find events that were written together
        /// </summary>
        public string CorrelationIdentifier { get; set; }

        /// <summary>
        /// Causation identifier used to find events that were written by the same thing (command or transaction)
        /// </summary>
        public string CausationIdentifier { get; set; }

        /// <summary>
        /// The name or URN of the schema used to write this event
        /// </summary>
        /// <remarks>
        /// If this is empty then the event type is used instead
        /// </remarks>
        public string SchemaName { get; set; }

        /// <summary>
        /// The date/time the event was written to the event stream
        /// </summary>
        public DateTime WriteTime { get; set; }

        private IEvent _eventInstance;
        /// <summary>
        /// The evnt instance as an ordinary object
        /// </summary>
        [JsonIgnore ]
        public IEvent EventInstance {
            get
            {
                if (null == _eventInstance )
                {
                    if (null != EventInstanceAsJson )
                    {
                        _eventInstance = EventInstanceAsJson.ToObject<EventInstance> ();
                    }
                }
                return _eventInstance;
            }
        }



        internal string ToJSonText()
        {
            return JsonConvert.SerializeObject(this, DefaultJSonSerialiserSettings());
        }

        /// <summary>
        /// The common serialiser settings for readinr or writing the wrapped event
        /// </summary>
        /// <returns></returns>
        internal JsonSerializerSettings DefaultJSonSerialiserSettings()
        {
            return new JsonSerializerSettings() { Formatting = Formatting.Indented , TypeNameHandling= TypeNameHandling.Objects };
        }

        /// <summary>
        /// Create a wrapper for this JSON event
        /// </summary>
        /// <param name="eventTypeName">
        /// The type of event in this wrapper
        /// </param>
        /// <param name="sequenceNumber">
        /// The ordinal sequence of the event in the event stream
        /// </param>
        /// <param name="VersionNumber">
        /// The version number of the event schema
        /// </param>
        /// <param name="writeTime">
        /// The date/time the event was written to the event stream
        /// </param>
        /// <param name="eventInstance">
        /// Th eunderlying data for this event instance
        /// </param>
        /// <param name="context">
        /// Extra context information to be written in with the event 
        /// </param>
        public static BlobBlockJsonWrappedEvent Create(string eventTypeName,
            int sequenceNumber, 
            int versionNumber, 
            DateTime? writeTime, 
            IEvent eventInstance, 
            IWriteContext context)
        {

            if (string.IsNullOrWhiteSpace(eventTypeName) )
            {
                if (null != eventInstance)
                {
                    // Use the type name if no explicit name is passed in
                    eventTypeName = eventInstance.GetType().FullName;
                }
                else
                {
                    // Mark this as being just a place holder - used when we need to copy an event stream but "wipe out" some events
                    eventTypeName = "Placeholder event";
                }
            }

            if (! writeTime.HasValue )
            {
                writeTime = DateTime.UtcNow;
            }

            BlobBlockJsonWrappedEvent ret = new BlobBlockJsonWrappedEvent()
            {
                EventTypeName = eventTypeName,
                SequenceNumber = sequenceNumber ,
                VersionNumber = versionNumber
            };

            ret.WriteTime = writeTime.Value ;

            if (null != eventInstance )
            {
                ret.EventInstanceAsJson = JObject.FromObject(eventInstance);
            }

            if (null != context )
            {
                ret.CorrelationIdentifier = context.CorrelationIdentifier;
                ret.Commentary = context.Commentary;
            }

            return ret;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="rawStream">
        /// The stream of data to read events from
        /// </param>
        internal static IEnumerable<BlobBlockJsonWrappedEvent> FromBinaryStream(Stream rawStream)
        {
            string jsonText;

            using (System.IO.StreamReader sr = new StreamReader(rawStream))
            {
                string innerJSON = sr.ReadToEnd();

                if (!string.IsNullOrWhiteSpace(innerJSON))
                {
                    jsonText = @"{ ""events"": [ " + innerJSON.Replace(@"}{", @"},{") + @" ] }";

                    JObject jsonO = JObject.Parse(jsonText);
                    if (null != jsonO)
                    {
                        JArray ja = (JArray)jsonO["events"];
                        if (null != ja)
                        {
                            return ja.ToObject<IEnumerable<BlobBlockJsonWrappedEvent>>();
                        }
                    }
                }
            }

            return Enumerable.Empty<BlobBlockJsonWrappedEvent>();
        }
    }
}
