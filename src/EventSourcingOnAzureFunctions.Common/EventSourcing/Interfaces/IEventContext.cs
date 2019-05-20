using System;
using System.Collections.Generic;
using System.Text;

namespace EventSourcingOnAzureFunctions.Common.EventSourcing.Interfaces
{
    /// <summary>
    /// Event and its context information provided when it was written
    /// </summary>
    public interface IEventContext
        : IEvent, IWriteContext 
    {

    }
}
