using System;
using System.Collections.Generic;
using System.Text;

namespace EventSourcingOnAzureFunctions.Common.CQRS.ClassifierHandler.Events
{
    public interface IClassifierRequest
    {

        /// <summary>
        /// The domain name of the event stream over which the projection is 
        /// to be run
        /// </summary>
        string DomainName { get; set; }

        /// <summary>
        /// The entity type for which the classifier will be run
        /// </summary>
        string EntityTypeName { get; set; }

        /// <summary>
        /// The unique instance of the event stream over which the 
        /// classifier should run
        /// </summary>
        string InstanceKey { get; set; }

        /// <summary>
        /// The name of the classifier to run over that event stream
        /// </summary>
        string ClassifierTypeName { get; set; }

        /// <summary>
        /// The date up-to which we want the classifier to be run
        /// </summary>
        Nullable<DateTime> AsOfDate { get; set; }


    }
}
