using System;
using EventSourcingOnAzureFunctions.Common.EventSourcing.Interfaces;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace EventSourcingOnAzureFunctions.Common.EventSourcing.Implementation.AzureStorage.AppendBlob
{
    [JsonObject(Title = "")]
    public class BlobBlockJsonWrappedEvent
        : IJsonSerialisedEvent
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


        public JObject EventInstanceAsJson => throw new NotImplementedException();


    }
}
