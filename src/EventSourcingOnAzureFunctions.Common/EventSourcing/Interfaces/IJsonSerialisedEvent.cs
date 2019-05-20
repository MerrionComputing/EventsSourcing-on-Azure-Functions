namespace EventSourcingOnAzureFunctions.Common.EventSourcing.Interfaces
{
    /// <summary>
    /// An event that is serialised to/from JSON in order to be persisted to an event stream
    /// </summary>
    /// <remarks>
    /// This adds an explicit property for the event class name to make it independent of how the 
    /// programming language chosen names its classes - you could choose to use a domain ubiquitous
    /// language event name for this
    /// </remarks>
    public interface IJsonSerialisedEvent
        : IEvent 
    {

        /// <summary>
        /// The name of the type of event this is
        /// </summary>
        /// <remarks>
        /// This allows projections to skip-over this type of event if they do not need to process it
        /// </remarks>
        string EventTypeName { get; }

        /// <summary>
        /// The underlying event data as a JSON object
        /// </summary>
        Newtonsoft.Json.Linq.JObject EventInstanceAsJson { get; }

    }
}
