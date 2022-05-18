using EventSourcingOnAzureFunctions.Common.EventSourcing.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EventSourcingOnAzureFunctions.Common.Binding
{
    /// <summary>
    /// The context passed in when a "new entity created" event is triggered
    /// </summary>
    public sealed class NewEntityContext
        : IEventStreamIdentity
    {

        /// <summary>
        /// The name of the domain for which this new entity was created
        /// </summary>
        public string DomainName { get; private set; }

        /// <summary>
        /// The name of the type of entity of which this new instance was created
        /// </summary>
        public string EntityTypeName { get; private set; }

        /// <summary>
        /// The unique identifier of the new entity that was created
        /// </summary>
        public string InstanceKey { get; private set; }

    }



}

