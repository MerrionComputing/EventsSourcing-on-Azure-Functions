using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace EventSourcingOnAzureFunctions.Common.EventSourcing.Interfaces
{
    interface IProjectionProcessor
    {


        /// <summary>
        /// Does the event stream over which this projection is slated to run exist
        /// </summary>
        Task<bool> Exists();

        /// <summary>
        /// Run the given projection over the underlying event stream
        /// </summary>
        /// <typeparam name="TProjection">
        /// The type of projection to run
        /// </typeparam>
        Task<TProjection> Process<TProjection>() where TProjection : IProjection, new();
    }
}
