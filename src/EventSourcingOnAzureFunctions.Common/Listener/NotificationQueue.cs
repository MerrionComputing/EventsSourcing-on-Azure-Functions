using System;
using System.Collections.Concurrent;
using System.Text;

namespace EventSourcingOnAzureFunctions.Common.Listener
{
    /// <summary>
    /// The notification queue is where the notifications for events (new entity created or new event written) are posted 
    /// so that notification listeners can react to them 
    /// </summary>
    public sealed class NotificationQueue
    {
    }
}
