using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;

using EventSourcingOnAzureFunctions.Common.Binding;
using EventSourcingOnAzureFunctions.Common.EventSourcing;

using RetailBank.AzureFunctionApp.Account.Projections;
using System;
using static EventSourcingOnAzureFunctions.Common.EventSourcing.Implementation.EventStreamBase;
using EventSourcingOnAzureFunctions.Common.EventSourcing.Exceptions;

namespace RetailBank.AzureFunctionApp
{
    public static class AccountFunctions
    {

        /// <summary>
        /// Open a new bank account
        /// </summary>
        /// <param name="accountnumber">
        /// The account number to use for the account.  If this already exists this command will return an error.
        /// </param>
        /// <returns></returns>
        [FunctionName("OpenAccount")]
        public static async Task<HttpResponseMessage> OpenAccountRun(
                      [HttpTrigger(AuthorizationLevel.Function, "POST", Route = @"OpenAccount/{accountnumber}")]HttpRequestMessage req,
                      string accountnumber,
                      [EventStream("Bank", "Account", "{accountnumber}")]  EventStream bankAccountEvents)
        {
            if (await bankAccountEvents.Exists())
            {
                return req.CreateResponse(System.Net.HttpStatusCode.Forbidden, $"Account {accountnumber} already exists");
            }
            else
            {
                // Get request body
                AccountOpeningData data = await req.Content.ReadAsAsync<AccountOpeningData>();

                // Append a "created" event
                DateTime dateCreated = DateTime.UtcNow;
                Account.Events.Opened evtOpened = new Account.Events.Opened() { LoggedOpeningDate = dateCreated };
                if (!string.IsNullOrWhiteSpace(data.Commentary))
                {
                    evtOpened.Commentary = data.Commentary;
                }
                try
                {
                    await bankAccountEvents.AppendEvent(evtOpened,
                        streamConstraint: EventStreamExistenceConstraint.MustBeNew
                        );
                }
                catch (EventStreamWriteException exWrite)
                {
                    return req.CreateResponse(System.Net.HttpStatusCode.Conflict, $"Account {accountnumber} had a conflict error on creation {exWrite.Message }");
                }

                // If there is an initial deposit in the account opening data, append a "deposit" event
                if (data.OpeningBalance.HasValue)
                {
                    Account.Events.MoneyDeposited evtInitialDeposit = new Account.Events.MoneyDeposited()
                    {
                        AmountDeposited = data.OpeningBalance.Value,
                        LoggedDepositDate = dateCreated,
                        Commentary = "Opening deposit"
                    };
                    await bankAccountEvents.AppendEvent(evtInitialDeposit);
                }

                // If there is a beneficiary in the account opening data append a "beneficiary set" event
                if (!string.IsNullOrEmpty(data.ClientName))
                {
                    Account.Events.BeneficiarySet evtBeneficiary = new Account.Events.BeneficiarySet()
                    { BeneficiaryName = data.ClientName };
                    await bankAccountEvents.AppendEvent(evtBeneficiary);
                }

                return req.CreateResponse(System.Net.HttpStatusCode.Created, $"Account {accountnumber} created");
            }
        }

        /// <summary>
        /// Get the current balance of a bank account
        /// </summary>
        /// <param name="req"></param>
        /// <param name="accountnumber">
        /// The account number of the account for which we want the balance
        /// </param>
        /// <param name="prjBankAccountBalance">
        /// The projection instance that is run to get the current account balance
        /// </param>
        /// <returns></returns>
        [FunctionName("GetBalance")]
        public static async Task<HttpResponseMessage> GetBalanceRun(
          [HttpTrigger(AuthorizationLevel.Function, "GET", Route = @"GetBalance/{accountnumber}" )]HttpRequestMessage req,
          string accountnumber,
          [Projection("Bank", "Account", "{accountnumber}", nameof(Balance))] Projection prjBankAccountBalance)
        {

            string result = $"No balance found for account {accountnumber}";

            if (null != prjBankAccountBalance)
            {
                if (await prjBankAccountBalance.Exists())
                {
                    // Get request body
                    Nullable<DateTime> asOfDate = null;
                    if (null != req.Content)
                    {
                        dynamic data = await req.Content.ReadAsAsync<object>();
                        if (null != data)
                        {
                            asOfDate = data.AsOfDate;
                        }
                    }
                    
                    Balance projectedBalance = await prjBankAccountBalance.Process<Balance>(asOfDate );
                    if (null != projectedBalance)
                    {
                        result = $"Balance for account {accountnumber} is ${projectedBalance.CurrentBalance} (As at record {projectedBalance.CurrentSequenceNumber}) ";
                    }
                }
                else
                {
                    result = $"Account {accountnumber} is not yet created - cannot retrieve a balance for it";
                }
            }

            return req.CreateResponse(System.Net.HttpStatusCode.OK, result);
        }


        [FunctionName("DepositMoney")]
        public static async Task<HttpResponseMessage> DepositMoneyRun(
              [HttpTrigger(AuthorizationLevel.Function, "POST", Route = @"DepositMoney/{accountnumber}")]HttpRequestMessage req,
              string accountnumber,
              [EventStream("Bank", "Account", "{accountnumber}")]  EventStream bankAccountEvents)
        {
            if (!await bankAccountEvents.Exists())
            {
                return req.CreateResponse(System.Net.HttpStatusCode.Forbidden, $"Account {accountnumber} does not exist");
            }
            else
            {
                // get the request body...
                MoneyDepositData data = await req.Content.ReadAsAsync<MoneyDepositData>();

                // create a deposited event
                DateTime dateDeposited = DateTime.UtcNow;
                Account.Events.MoneyDeposited evDeposited = new Account.Events.MoneyDeposited()
                {
                    LoggedDepositDate = dateDeposited,
                    AmountDeposited = data.DepositAmount,
                    Commentary = data.Commentary,
                    Source = data.Source
                };

                await bankAccountEvents.AppendEvent(evDeposited);

                return req.CreateResponse(System.Net.HttpStatusCode.OK, $"{data.DepositAmount} deposited to account {accountnumber} ");
            }
        }


        // WithdrawMoney
        [FunctionName("WithdrawMoney")]
        public static async Task<HttpResponseMessage> WithdrawMoneyRun(
              [HttpTrigger(AuthorizationLevel.Function, "POST", Route = @"WithdrawMoney/{accountnumber}")]HttpRequestMessage req,
              string accountnumber,
              [EventStream("Bank", "Account", "{accountnumber}")]  EventStream bankAccountEvents,
              [Projection("Bank", "Account", "{accountnumber}", nameof(Balance))] Projection prjBankAccountBalance,
              [Projection("Bank", "Account", "{accountnumber}", nameof(OverdraftLimit))] Projection prjBankAccountOverdraft)
        {
            if (!await bankAccountEvents.Exists())
            {
                return req.CreateResponse(System.Net.HttpStatusCode.Forbidden, $"Account {accountnumber} does not exist");
            }
            else
            {
                // get the request body...
                MoneyWithdrawnData  data = await req.Content.ReadAsAsync<MoneyWithdrawnData>();

                // get the current account balance
                Balance projectedBalance = await prjBankAccountBalance.Process<Balance>();
                if (null != projectedBalance)
                {
                    OverdraftLimit projectedOverdraft = await prjBankAccountOverdraft.Process<OverdraftLimit>();

                    decimal overdraftSet = 0.00M;
                    if (null != projectedOverdraft )
                    {
                        if (projectedOverdraft.CurrentSequenceNumber != projectedBalance.CurrentSequenceNumber   )
                        {
                            // The two projectsions are out of synch.  In a real business case we would retry them 
                            // n times to try and get a match but here we will just throw a consistency error
                            return req.CreateResponse(System.Net.HttpStatusCode.Forbidden, 
                                $"Unable to get a matching state for the current balance and overdraft for account {accountnumber}");
                        }
                        else
                        {
                            overdraftSet = projectedOverdraft.CurrentOverdraftLimit; 
                        }
                    }

                    if ((projectedBalance.CurrentBalance + overdraftSet) >= data.AmountWithdrawn)
                    {
                        // attempt the withdrawal
                        DateTime dateWithdrawn = DateTime.UtcNow;
                        Account.Events.MoneyWithdrawn evWithdrawn = new Account.Events.MoneyWithdrawn()
                        {
                            LoggedWithdrawalDate = dateWithdrawn,
                            AmountWithdrawn = data.AmountWithdrawn ,
                            Commentary = data.Commentary 
                        };
                        try
                        {
                            await bankAccountEvents.AppendEvent(evWithdrawn, projectedBalance.CurrentSequenceNumber);
                        }
                        catch (EventSourcingOnAzureFunctions.Common.EventSourcing.Exceptions.EventStreamWriteException exWrite  )
                        {
                            return req.CreateResponse(System.Net.HttpStatusCode.Forbidden, 
                                $"Failed to write withdrawal event {exWrite.Message}");
                        }
                        return req.CreateResponse(System.Net.HttpStatusCode.OK, 
                            $"{data.AmountWithdrawn } withdrawn from account {accountnumber} (New balance: {projectedBalance.CurrentBalance - data.AmountWithdrawn}, overdraft: {overdraftSet} )");
                    }
                    else
                    {
                        return req.CreateResponse(System.Net.HttpStatusCode.Forbidden, 
                            $"Account {accountnumber} does not have sufficent funds for the withdrawal of {data.AmountWithdrawn} (Current balance: {projectedBalance.CurrentBalance}, overdraft: {overdraftSet} )");
                    }
                }
                else
                {
                    return req.CreateResponse(System.Net.HttpStatusCode.Forbidden, $"Unable to get current balance for account {accountnumber}");
                }
            }
        }


        /// <summary>
        /// Set or change who is the beneficial owner of this account
        /// </summary>
        /// <returns></returns>
        [FunctionName("SetBeneficialOwner") ]
        public static async Task<HttpResponseMessage> SetBeneficialOwnerRun(
              [HttpTrigger(AuthorizationLevel.Function, "POST", Route = "SetBeneficialOwner/{accountnumber}/{ownername}")]HttpRequestMessage req,
              string accountnumber,
              string ownername,
              [EventStream("Bank", "Account", "{accountnumber}")]  EventStream bankAccountEvents)
        {
            if (await bankAccountEvents.Exists())
            {
                if (!string.IsNullOrEmpty(ownername))
                {
                    Account.Events.BeneficiarySet evtBeneficiary = new Account.Events.BeneficiarySet()
                    { BeneficiaryName = ownername };
                    await bankAccountEvents.AppendEvent(evtBeneficiary);
                }

                return req.CreateResponse(System.Net.HttpStatusCode.Created, $"Beneficial owner of account {accountnumber} set");
            }
            else
            {
                return req.CreateResponse(System.Net.HttpStatusCode.Forbidden, $"Account {accountnumber} does not exist");
            }
        }


        /// <summary>
        /// Set a new overdraft limit for the account
        /// </summary>
        [FunctionName("SetOverdraftLimit")]
        public static async Task<HttpResponseMessage> SetOverdraftLimitRun(
          [HttpTrigger(AuthorizationLevel.Function, "POST", Route = @"SetOverdraftLimit/{accountnumber}")]HttpRequestMessage req,
          string accountnumber,
          [EventStream("Bank", "Account", "{accountnumber}")]  EventStream bankAccountEvents,
          [Projection("Bank", "Account", "{accountnumber}", nameof(Balance))] Projection prjBankAccountBalance)
        {
            if (!await bankAccountEvents.Exists())
            {
                // You cannot set an overdraft if the account does not exist
                return req.CreateResponse(System.Net.HttpStatusCode.Forbidden, $"Account {accountnumber} does not exist");
            }
            else
            {
                // get the request body...
                OverdraftSetData data = await req.Content.ReadAsAsync<OverdraftSetData>();

                // get the current account balance
                Balance projectedBalance = await prjBankAccountBalance.Process<Balance>();
                if (null != projectedBalance)
                {
                    if (projectedBalance.CurrentBalance >= (0 - data.NewOverdraftLimit) )
                    {
                        // attempt to set the new overdraft limit
                        Account.Events.OverdraftLimitSet evOverdraftSet = new Account.Events.OverdraftLimitSet()
                        {
                            OverdraftLimit = data.NewOverdraftLimit ,
                            Commentary = data.Commentary
                        };
                        try
                        {
                            await bankAccountEvents.AppendEvent(evOverdraftSet, projectedBalance.CurrentSequenceNumber);
                        }
                        catch (EventSourcingOnAzureFunctions.Common.EventSourcing.Exceptions.EventStreamWriteException exWrite)
                        {
                            return req.CreateResponse(System.Net.HttpStatusCode.Forbidden, $"Failed to write overdraft limit event {exWrite.Message}");
                        }
                        return req.CreateResponse(System.Net.HttpStatusCode.OK, $"{data.NewOverdraftLimit } set as the new overdraft limit for account {accountnumber} ");
                    }
                    else
                    {
                        return req.CreateResponse(System.Net.HttpStatusCode.Forbidden, $"Account {accountnumber} has an outstanding balance greater than the new limit {data.NewOverdraftLimit} (Current balance: {projectedBalance.CurrentBalance} )");
                    }
                }
                else
                {
                    return req.CreateResponse(System.Net.HttpStatusCode.Forbidden, $"Unable to get current balance for account {accountnumber}");
                }
            }
        }
    }
}
