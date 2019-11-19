using System;
using System.Collections.Generic;
using System.Text;

namespace EventSourcingOnAzureFunctions.Common.EventSourcing.Interfaces
{
    public interface IClassifyEventType<TEventType>
    {
        /// <summary>
        /// Classify the given instance of this event type
        /// </summary>
        /// <param name="eventInstance">
        /// The specific instance of this event type with its data properties set
        /// </param>
        Classification.ClassificationResults   ClassifyEventInstance(TEventType eventInstance);
    }
}
