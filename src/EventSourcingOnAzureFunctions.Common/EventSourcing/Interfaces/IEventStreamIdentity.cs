using System;
using System.Collections.Generic;
using System.Text;

namespace EventSourcingOnAzureFunctions.Common.EventSourcing.Interfaces
{
    /// <summary>
    /// An interface that can be used to uniquely identify an event stream instance
    /// </summary>
    /// <remarks>
    /// The combination of DomainName.EntityTypeName.InstanceKey will uniquely identify the  entity instance
    /// e.g. HumanResources.Empolyee.Duncan_Jones could represent my records in a company HR system
    /// </remarks>
    public interface IEventStreamIdentity
    {

        /// <summary>
        /// The domain to which the entity (and its event stream) belong
        /// </summary>
        string DomainName { get; }

        /// <summary>
        /// The type/classification of entity to which the entity (and its event stream) belong
        /// </summary>
        string EntityTypeName { get; }

        /// <summary>
        /// The unique identifier of the aggregate instance to which the event stream pertains
        /// </summary>
        string InstanceKey { get; }

    }
}
