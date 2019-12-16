using EventSourcingOnAzureFunctions.Common.EventSourcing;
using EventSourcingOnAzureFunctions.Common.EventSourcing.Interfaces;
using RetailBank.AzureFunctionApp.Account.Events;
using System;

namespace RetailBank.AzureFunctionApp.Account.Classifications
{
    /// <summary>
    /// A classification of a bank account to state whether that account has had interest 
    /// accrued today
    /// </summary>
    public class InterestAccruedToday
        : ClassificationBase,
        IClassifyEventType<InterestAccrued>
    {

        public ClassificationResponse.ClassificationResults ClassifyEventInstance(InterestAccrued eventInstance)
        {
            // if it happened today set it to true
            if (eventInstance.AccrualEffectiveDate.Date == DateTime.Today  )
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
