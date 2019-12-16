using EventSourcingOnAzureFunctions.Common.EventSourcing;
using EventSourcingOnAzureFunctions.Common.EventSourcing.Interfaces;
using RetailBank.AzureFunctionApp.Account.Events;
using System;
using System.Collections.Generic;
using System.Text;

namespace RetailBank.AzureFunctionApp.Account.Classifications
{
    /// <summary>
    /// True if the current balance of this account is below a given threshold
    /// </summary>
    /// <remarks>
    /// This is an example of a classifier that uses a parameter
    /// </remarks>
    public class BalanceBelowGivenThreshold
        : ClassificationBase,
          IClassifyEventType<MoneyDeposited>,
          IClassifyEventType<MoneyWithdrawn>,
          IClassifyEventType<InterestPaid>
    {


        public const string PARAMETER_BALANCE_THRESHOLD = @"Balance Threshold";

        private decimal threshold;
        private decimal currentBalance;


        public override void SetParameter(string parameterName, object parameterValue)
        {
            if (parameterName == PARAMETER_BALANCE_THRESHOLD )
            {
                if (null != parameterValue )
                {
                    threshold = (decimal)parameterValue;
                }
            }
        }

        public  ClassificationResponse.ClassificationResults ClassifyEventInstance(MoneyDeposited eventInstance)
        {
            if (null != eventInstance)
            {
                currentBalance += eventInstance.AmountDeposited;
            }
            if (currentBalance >= threshold )
            {
                // Balance is above threshold
                return ClassificationResponse.ClassificationResults.Exclude;
            }
            else
            {
                return ClassificationResponse.ClassificationResults.Include;
            }
        }

        public  ClassificationResponse.ClassificationResults ClassifyEventInstance(MoneyWithdrawn eventInstance)
        {
            if (null != eventInstance)
            {
                currentBalance -= eventInstance.AmountWithdrawn;
            }
            if (currentBalance >= threshold)
            {
                // Balance is above threshold
                return ClassificationResponse.ClassificationResults.Exclude;
            }
            else
            {
                return ClassificationResponse.ClassificationResults.Include;
            }
        }

        public  ClassificationResponse.ClassificationResults ClassifyEventInstance(InterestPaid eventInstance)
        {
            if (null != eventInstance)
            {
                currentBalance += eventInstance.AmountPaid;
            }
            if (currentBalance >= threshold)
            {
                // Balance is above threshold
                return ClassificationResponse.ClassificationResults.Exclude;
            }
            else
            {
                return ClassificationResponse.ClassificationResults.Include;
            }
        }

    }
}
