using System;
using System.Collections.Generic;
using System.Text;
using EventSourcingOnAzureFunctions.Common.CQRS.CommandHandler.Events;
using EventSourcingOnAzureFunctions.Common.EventSourcing;
using EventSourcingOnAzureFunctions.Common.EventSourcing.Interfaces;

namespace EventSourcingOnAzureFunctions.Common.CQRS.CommandHandler.Classifications
{
    /// <summary>
    /// A classification to denote that a command execution has had a fatal error
    /// </summary>
    [ClassificationName("Command In Error")]
    public sealed class InError
        : ClassificationBase,
        IClassifyEventType<FatalErrorOccured>
    {
        public ClassificationResponse.ClassificationResults ClassifyEventInstance(FatalErrorOccured eventInstance)
        {
            if (eventInstance != null)
            {
                return ClassificationResponse.ClassificationResults.Include;
            }
            return ClassificationResponse.ClassificationResults.Unchanged;
        }

        public override void SetParameter(string parameterName, object parameterValue)
        {
            // This classifier has no parameters
        }
    }
}
