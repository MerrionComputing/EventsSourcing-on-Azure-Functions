

using EventSourcingOnAzureFunctions.Common.Binding;
using EventSourcingOnAzureFunctions.Common.EventSourcing;

using Microsoft.Azure.WebJobs.Extensions.DurableTask;

using Microsoft.Azure.WebJobs;
using System.Threading.Tasks;
using System.Collections.Generic;
using RetailBank.AzureFunctionApp.Account.Projections;
using RetailBank.AzureFunctionApp.Account.Classifications;
using System;

namespace RetailBank.AzureFunctionApp
{
    /// <summary>
    /// Functions that rely on the Durable Functions library for orchestration
    /// </summary>
    public partial class AccountFunctions
    {

        // 1 - Accrue interest for all 
        // Triggered by a timer "0 30 1 * * *"  at 01:30 AM every day
        [FunctionName(nameof(AccrueInterestFoAllAccountsTimer))]
        public static async Task AccrueInterestFoAllAccountsTimer(
            [TimerTrigger("0 30 1 * * *", 
            RunOnStartup=false,
            UseMonitor =true)] TimerInfo accrueInterestTimer,
            [DurableClient] IDurableOrchestrationClient accrueInterestOrchestration,
            [Classification("Bank", "Account", "ALL", @"")] Classification clsAllAccounts
             )
        {
            // Get all the account numbers
            IEnumerable<string> allAccounts = await clsAllAccounts.GetAllInstanceKeys();

            await accrueInterestOrchestration.StartNewAsync(nameof(AccrueInterestForAllAccounts), allAccounts);
        }

        // Accrue Interest For All Accounts
        [FunctionName(nameof(AccrueInterestForAllAccounts))]
        public static async Task AccrueInterestForAllAccounts
            ([OrchestrationTrigger] IDurableOrchestrationContext context)
        {

            IEnumerable<string> allAccounts = context.GetInput<IEnumerable<string>>();
            
            if (null != allAccounts )
            {
                var accrualTasks = new List<Task<Tuple<string, bool>>>();
                foreach (string accountNumber in allAccounts)
                {
                    Task<Tuple<string, bool>> accrualTask = context.CallActivityAsync<Tuple<string, bool>>(nameof(AccrueInterestForSpecificAccount), accountNumber );
                    accrualTasks.Add(accrualTask);
                }

                // Perform all the accruals in parrallel
                await Task.WhenAll(accrualTasks);

                List<string> failedAccruals = new List<string>();
                foreach (var accrualTask in accrualTasks)
                {
                    if (! accrualTask.Result.Item2  )
                    {
                        failedAccruals.Add(accrualTask.Result.Item1); 
                    }
                }

                // Try a second pass - using failedAccruals.Count ?
                if (failedAccruals.Count > 0 )
                {
                    throw new Exception("Not all account accruals succeeded");
                }

            }

        }

        //AccrueInterestForSpecificAccount
        [FunctionName(nameof(AccrueInterestForSpecificAccount))]
        public static async Task<Tuple<string, bool>> AccrueInterestForSpecificAccount
            ([ActivityTrigger] IDurableActivityContext accrueInterestContext)
        {

            const decimal DEBIT_INTEREST_RATE = 0.01M;
            const decimal CREDIT_INTEREST_RATE = 0.005M;

            string accountNumber = accrueInterestContext.GetInput<string>();

            if (!string.IsNullOrEmpty(accountNumber))
            {
                EventStream bankAccountEvents = new EventStream(new EventStreamAttribute("Bank", "Account", accountNumber));
                if (await bankAccountEvents.Exists() )
                {
                    // Has the accrual been done today for this account?
                    Classification clsAccruedToday = new Classification(new ClassificationAttribute("Bank", "Account", accountNumber, nameof(InterestAccruedToday)));
                    ClassificationResponse isAccrued = await clsAccruedToday.Classify<InterestAccruedToday>();
                    if (isAccrued.Result != ClassificationResponse.ClassificationResults.Include)
                    {
                        // Get the account balance
                        Projection prjBankAccountBalance = new Projection(new ProjectionAttribute("Bank", "Account", accountNumber, nameof(Balance)));

                        // Get the current account balance, as at midnight
                        Balance projectedBalance = await prjBankAccountBalance.Process<Balance>(DateTime.Today);
                        if (null != projectedBalance)
                        {
                            Account.Events.InterestAccrued evAccrued = new Account.Events.InterestAccrued()
                            {
                                Commentary = $"Daily scheduled interest accrual",
                                AccrualEffectiveDate = DateTime.Today  // set the accrual to midnight today  
                            };
                            // calculate the accrual amount
                            if (projectedBalance.CurrentBalance >= 0)
                            {
                                // Using the credit rate
                                evAccrued.AmountAccrued = CREDIT_INTEREST_RATE  * projectedBalance.CurrentBalance;
                                evAccrued.InterestRateInEffect = CREDIT_INTEREST_RATE;
                            }
                            else
                            {
                                // Use the debit rate
                                evAccrued.AmountAccrued = DEBIT_INTEREST_RATE * projectedBalance.CurrentBalance;
                                evAccrued.InterestRateInEffect = DEBIT_INTEREST_RATE;
                            }

                            try
                            {
                                await bankAccountEvents.AppendEvent(evAccrued, isAccrued.AsOfSequence);
                            }
                            catch (EventSourcingOnAzureFunctions.Common.EventSourcing.Exceptions.EventStreamWriteException exWrite)
                            {
                                // We can't be sure this hasn't already run... 
                                return new Tuple<string,bool>(accountNumber, false);
                            }
                            
                        }
                    }
                }
            }
            return new Tuple<string, bool>(accountNumber, true);
        }
    }
}
