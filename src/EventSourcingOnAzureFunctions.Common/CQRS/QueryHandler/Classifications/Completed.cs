using EventSourcingOnAzureFunctions.Common.EventSourcing;
using EventSourcingOnAzureFunctions.Common.EventSourcing.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace EventSourcingOnAzureFunctions.Common.CQRS.QueryHandler.Classifications
{
    /// <summary>
    /// A classification to denote that a query execution has completed
    /// </summary>
    public class Completed: 
        ClassificationBase,
        IClassifyEventType<Completed>
    {

        public ClassificationResponse.ClassificationResults ClassifyEventInstance(Completed eventInstance)
        {
            return ClassificationResponse.ClassificationResults.Include;
        }

        public override void SetParameter(string parameterName, object parameterValue)
        {
            // This classifier has no parameters
        }
    }
}
