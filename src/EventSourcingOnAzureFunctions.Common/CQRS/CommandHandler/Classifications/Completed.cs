﻿using EventSourcingOnAzureFunctions.Common.CQRS.CommandHandler.Events;
using EventSourcingOnAzureFunctions.Common.EventSourcing;
using EventSourcingOnAzureFunctions.Common.EventSourcing.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace EventSourcingOnAzureFunctions.Common.CQRS.CommandHandler.Classifications
{
    /// <summary>
    /// A classification to denote that a command execution has completed
    /// </summary>
    [ClassificationName("Command Completed") ]
    public sealed class Completed
        : ClassificationBase,
        IClassifyEventType<Completed>
    {


        public ClassificationResponse.ClassificationResults ClassifyEventInstance(Completed eventInstance)
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
