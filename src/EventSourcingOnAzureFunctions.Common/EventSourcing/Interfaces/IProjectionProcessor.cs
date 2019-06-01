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
        /// <param name="asOfDate">
        /// If set, only run the projection up until this date/time
        /// </param>
        Task<TProjection> Process<TProjection>(DateTime? asOfDate = null) where TProjection : IProjection, new();
    }
}
