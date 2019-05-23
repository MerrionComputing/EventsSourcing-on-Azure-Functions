using EventSourcingOnAzureFunctions.Common.EventSourcing.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace EventSourcingOnAzureFunctions.Common.EventSourcing.Implementation
{
    public sealed class ProjectionProcessor
        : IProjectionProcessor
    {

        private readonly IEventStreamReader eventStreamReader = null;

        public async Task<TProjection> Process<TProjection>() where TProjection : IProjection, new()
        {
            TProjection ret = new TProjection();

            if (null != eventStreamReader)
            {
                foreach (var item in await eventStreamReader.GetEventsWithContext())
                {
                    //ret.OnEventRead(item.SequenceNumber, EventAsOfDateAttribute.GetAsOfDate(item.e))

                    // mark the event as handled
                    ret.MarkEventHandled(item.SequenceNumber);
                }
            }

            return  ret;
        }
    }
}
