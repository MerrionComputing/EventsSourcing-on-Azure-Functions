using EventSourcingOnAzureFunctions.Common.CommandHandler.Events;
using EventSourcingOnAzureFunctions.Common.EventSourcing;
using EventSourcingOnAzureFunctions.Common.EventSourcing.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace EventSourcingOnAzureFunctions.Common.CommandHandler.Classifications
{
    /// <summary>
    /// A classification to denote that a command execution has completed
    /// </summary>
    public class Completed
        : ClassificationBase,
        IClassifyEventType<CommandHandler.Events.Completed>
    {


        public ClassificationResponse.ClassificationResults ClassifyEventInstance(Events.Completed eventInstance)
        {
            return ClassificationResponse.ClassificationResults.Include;
        }
    }
}
