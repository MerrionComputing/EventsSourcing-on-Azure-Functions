using System;
using System.Collections.Generic;
using System.Text;

namespace EventSourcingOnAzureFunctions.Common.EventSourcing
{

    [AttributeUsage(AttributeTargets.Class , AllowMultiple = false)]
    public sealed class EventAsOfDateAttribute
        : Attribute
    {
    }
}
