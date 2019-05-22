using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace EventSourcingOnAzureFunctions.Common.EventSourcing.Interfaces
{
    interface IProjectionProcessor
    {


        Task<TProjection> Process<TProjection>() where TProjection : IProjection, new();

    }
}
