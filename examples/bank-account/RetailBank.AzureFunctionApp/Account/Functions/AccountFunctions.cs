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
using RetailBank.AzureFunctionApp.Account.Classifications;
using RetailBank.AzureFunctionApp.Account.Events;
using System.Collections.Generic;
using System.Diagnostics;

namespace RetailBank.AzureFunctionApp
{
    public partial class AccountFunctions
    {

        #region Open a new account
        /// <summary>
        /// Open a new bank account
        /// </summary>
        /// <param name="accountnumber">
        /// The account number to use for the account.  
        /// If this already exists this command will return an error.
        /// </param>
        /// <param name="bankAccountEvents">
        /// The event stream to create and append events to
        /// </param>
        [FunctionName("OpenAccount")]
        public static async Task<HttpResponseMessage> OpenAccountRun(
                      [HttpTrigger(AuthorizationLevel.Function, "POST", Route = @"OpenAccount/{accountnumber}")]HttpRequestMessage req,
                      string accountnumber,
                      [EventStream("Bank", "Account", "{accountnumber}")]  EventStream bankAccountEvents)
        {

            // Set the start time for how long it took to process the message
            DateTime startTime = DateTime.UtcNow;

            #region Tracing telemetry
            Activity.Current.AddTag("Account Number", accountnumber);
            #endregion

            if (await bankAccountEvents.Exists())
            {
                return req.CreateResponse<FunctionResponse>(System.Net.HttpStatusCode.Forbidden,
                    FunctionResponse.CreateResponse(startTime, 
                    true,
                    $"Account {accountnumber} already exists"),
                    FunctionResponse.MEDIA_TYPE);
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
                    return req.CreateResponse<FunctionResponse>(System.Net.HttpStatusCode.Conflict,
                        FunctionResponse.CreateResponse(startTime,
                        true,
                        $"Account {accountnumber} had a conflict error on creation {exWrite.Message }"),
                        FunctionResponse.MEDIA_TYPE);
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

                return req.CreateResponse<FunctionResponse>(System.Net.HttpStatusCode.Created,
                    FunctionResponse.CreateResponse(startTime,
                    false,
                    $"Account { accountnumber} created"),
                    FunctionResponse.MEDIA_TYPE);

            }
        }
        #endregion 

        #region Delete an account
        [FunctionName("DeleteAccount")]
        public static async Task<HttpResponseMessage> DeleteAccountRun(
                      [HttpTrigger(AuthorizationLevel.Function, "POST", Route = @"DeleteAccount/{accountnumber}")]HttpRequestMessage req,
                      string accountnumber,
                      [EventStream("Bank", "Account", "{accountnumber}")]  EventStream bankAccountEvents)
        {
            // Set the start time for how long it took to process the message
            DateTime startTime = DateTime.UtcNow;

            #region Tracing telemetry
            Activity.Current.AddTag("Account Number", accountnumber);
            #endregion

            if (!await bankAccountEvents.Exists())
            {
                return req.CreateResponse<FunctionResponse>(System.Net.HttpStatusCode.Forbidden,
                    FunctionResponse.CreateResponse(startTime,
                    true,
                    $"Cannot delete account {accountnumber} as it does not exist"),
                    FunctionResponse.MEDIA_TYPE);
            }
            else
            {
                try
                {
                    await bankAccountEvents.DeleteStream(); 
                }
                catch (EventStreamWriteException exWrite)
                {
                    return req.CreateResponse<FunctionResponse>(System.Net.HttpStatusCode.Conflict,
                        FunctionResponse.CreateResponse(startTime,
                        true,
                        $"Account {accountnumber} had a conflict error on deletion {exWrite.Message }"),
                        FunctionResponse.MEDIA_TYPE);
                }

                return req.CreateResponse<FunctionResponse>(System.Net.HttpStatusCode.Created,
                FunctionResponse.CreateResponse(startTime,
                false,
                $"Account { accountnumber} deleted"),
                FunctionResponse.MEDIA_TYPE);

            }
        }

        #endregion 

        #region Get Account Balance

        /// <summary>
        /// Get the current balance of a bank account
        /// </summary>
        /// <param name="accountnumber">
        /// The account number of the account for which we want the balance
        /// </param>
        /// <param name="asOfDate">
        /// The effective date for which to get the balance
        /// </param>
        /// <param name="prjBankAccountBalance">
        /// The projection instance that is run to get the current account balance
        /// </param>
        [FunctionName("GetBalance")]
        public static async Task<HttpResponseMessage> GetBalanceRun(
          [HttpTrigger(AuthorizationLevel.Anonymous , "GET", Route = @"GetBalance/{accountnumber}/{asOfDate?}" )]HttpRequestMessage req,
          string accountnumber,
          string asOfDate,
          [Projection("Bank", "Account", "{accountnumber}", nameof(Balance))] Projection prjBankAccountBalance)
        {

            // Set the start time for how long it took to process the message
            DateTime startTime = DateTime.UtcNow;

            #region Tracing telemetry
            Activity.Current.AddTag("Account Number", accountnumber);
            #endregion

            string result = $"No balance found for account {accountnumber}";

            //see if a prior balance was passed in
            ExistingBalanceData priorBalance = null;
            if (req.Content != null)
            {
                if (! (req.Content.Headers.ContentType == null))
                {
                    priorBalance = await req.Content.ReadAsAsync<ExistingBalanceData>();
                }
            }

            if (null != prjBankAccountBalance)
            {
                if (await prjBankAccountBalance.Exists())
                {
                    // Get request body
                    Nullable<DateTime> asOfDateValue = null;
                    if (! string.IsNullOrEmpty(asOfDate) )
                    {
                        DateTime dtTest;
                        if( DateTime.TryParse(asOfDate, out dtTest ))
                        {
                            asOfDateValue = dtTest;
                        }
                    }

                    // Run the "Balance" projection
                    Balance startingBalance = null;
                    if (priorBalance != null)
                    {
                        startingBalance = new Balance(priorBalance.Balance, priorBalance.AsOfSequenceNumber);
                    }
                    else
                    {
                        startingBalance = new Balance();
                    }
                    

                    Balance projectedBalance = await prjBankAccountBalance.Process<Balance>(startingBalance, asOfDateValue);
                    if (null != projectedBalance)
                    {
                        result = $"Balance for account {accountnumber} is ${projectedBalance.CurrentBalance} ";
                        return req.CreateResponse<ProjectionFunctionResponse>(System.Net.HttpStatusCode.OK,
                                ProjectionFunctionResponse.CreateResponse(startTime,
                                false,
                                result,
                                projectedBalance.CurrentSequenceNumber ),
                                FunctionResponse.MEDIA_TYPE);
                    }
                }
                else
                {
                    result = $"Account {accountnumber} is not yet created - cannot retrieve a balance for it";
                    return req.CreateResponse<ProjectionFunctionResponse>(System.Net.HttpStatusCode.NotFound ,
                        ProjectionFunctionResponse.CreateResponse(startTime,
                        true,
                        result,
                        0),
                        FunctionResponse.MEDIA_TYPE);
                }
            }

            // If we got here no balance was found
            return req.CreateResponse<ProjectionFunctionResponse>(System.Net.HttpStatusCode.NotFound,
                ProjectionFunctionResponse.CreateResponse(startTime,
                true,
                result,
                0),
                FunctionResponse.MEDIA_TYPE);

        }
#endregion

        #region Deposit money
        /// <summary>
        /// Add money to the selected bank account
        /// </summary>
        /// <param name="accountnumber">
        /// The unique identifier of the account we are depositing money to
        /// </param>
        /// <param name="bankAccountEvents">
        /// The event stream to append the account event(s) onto
        /// </param>
        [FunctionName("DepositMoney")]
        public static async Task<HttpResponseMessage> DepositMoneyRun(
              [HttpTrigger(AuthorizationLevel.Anonymous , "POST", Route = @"DepositMoney/{accountnumber}")]HttpRequestMessage req,
              string accountnumber,
              [EventStream("Bank", "Account", "{accountnumber}")]  EventStream bankAccountEvents)
        {

            // Set the start time for how long it took to process the message
            DateTime startTime = DateTime.UtcNow;

            #region Tracing telemetry
            Activity.Current.AddTag("Account Number", accountnumber);
            #endregion

            if (!await bankAccountEvents.Exists())
            {
                return req.CreateResponse<FunctionResponse>(System.Net.HttpStatusCode.NotFound ,
                        FunctionResponse.CreateResponse(startTime,
                        true,
                        $"Account {accountnumber} does not exist"),
                        FunctionResponse.MEDIA_TYPE);
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

                return req.CreateResponse<FunctionResponse>(System.Net.HttpStatusCode.OK ,
                        FunctionResponse.CreateResponse(startTime,
                        false,
                        $"{data.DepositAmount} deposited to account {accountnumber} "),
                        FunctionResponse.MEDIA_TYPE);

            }
        }
        #endregion 

        #region Withdraw Money
        /// <summary>
        /// Withdraw money from the given bank account
        /// </summary>
        /// <param name="accountnumber">
        /// The account number to use
        /// </param>
        /// <param name="bankAccountEvents">
        /// The event stream to add events to the end of
        /// </param>
        /// <param name="prjBankAccountBalance">
        /// The projection to get the current bank balance
        /// </param>
        /// <param name="prjBankAccountOverdraft">
        /// The projection to get the current overdraft for
        /// </param>
        /// <returns></returns>
        [FunctionName("WithdrawMoney")]
        public static async Task<HttpResponseMessage> WithdrawMoneyRun(
              [HttpTrigger(AuthorizationLevel.Anonymous , "POST", Route = @"WithdrawMoney/{accountnumber}")]HttpRequestMessage req,
              string accountnumber,
              [EventStream("Bank", "Account", "{accountnumber}")]  EventStream bankAccountEvents,
              [Projection("Bank", "Account", "{accountnumber}", nameof(Balance))] Projection prjBankAccountBalance,
              [Projection("Bank", "Account", "{accountnumber}", nameof(OverdraftLimit))] Projection prjBankAccountOverdraft)
        {

            // Set the start time for how long it took to process the message
            DateTime startTime = DateTime.UtcNow;

            #region Tracing telemetry
            Activity.Current.AddTag("Account Number", accountnumber);
            #endregion

            if (!await bankAccountEvents.Exists())
            {
                return req.CreateResponse<ProjectionFunctionResponse>(System.Net.HttpStatusCode.NotFound,
                        ProjectionFunctionResponse.CreateResponse(startTime,
                        true,
                        $"Account {accountnumber} does not exist",
                        0),
                        FunctionResponse.MEDIA_TYPE);
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
                            // The two projections are out of synch.  In a real business case we would retry them 
                            // n times to try and get a match but here we will just throw a consistency error
                            return req.CreateResponse<ProjectionFunctionResponse>(System.Net.HttpStatusCode.Forbidden,
                                    ProjectionFunctionResponse.CreateResponse(startTime,
                                    true,
                                    $"Unable to get a matching state for the current balance and overdraft for account {accountnumber}",
                                    0),
                                    FunctionResponse.MEDIA_TYPE);
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
                            return req.CreateResponse<ProjectionFunctionResponse>(System.Net.HttpStatusCode.Forbidden,
                                    ProjectionFunctionResponse.CreateResponse(startTime,
                                    true,
                                    $"Failed to write withdrawal event {exWrite.Message}",
                                    0),
                                    FunctionResponse.MEDIA_TYPE);

                        }

                        return req.CreateResponse<ProjectionFunctionResponse>(System.Net.HttpStatusCode.OK,
                            ProjectionFunctionResponse.CreateResponse(startTime,
                            false,
                            $"{data.AmountWithdrawn } withdrawn from account {accountnumber} (New balance: {projectedBalance.CurrentBalance - data.AmountWithdrawn}, overdraft: {overdraftSet} )",
                            projectedBalance.CurrentSequenceNumber ),
                            FunctionResponse.MEDIA_TYPE);
                    }
                    else
                    {

                        return req.CreateResponse<ProjectionFunctionResponse>(System.Net.HttpStatusCode.Forbidden,
                                ProjectionFunctionResponse.CreateResponse(startTime,
                                true,
                                $"Account {accountnumber} does not have sufficent funds for the withdrawal of {data.AmountWithdrawn} (Current balance: {projectedBalance.CurrentBalance}, overdraft: {overdraftSet} )",
                                projectedBalance.CurrentSequenceNumber ),
                                FunctionResponse.MEDIA_TYPE);

                            
                    }
                }
                else
                {
                    return req.CreateResponse<ProjectionFunctionResponse>(System.Net.HttpStatusCode.Forbidden,
                                ProjectionFunctionResponse.CreateResponse(startTime,
                                true,   
                                $"Unable to get current balance for account {accountnumber}", 
                                projectedBalance.CurrentSequenceNumber),
                                FunctionResponse.MEDIA_TYPE);
                }
            }
        }

        #endregion

        #region Set account owner
        /// <summary>
        /// Set or change who is the beneficial owner of this account
        /// </summary>
        /// <returns></returns>
        [FunctionName("SetBeneficialOwner") ]
        public static async Task<HttpResponseMessage> SetBeneficialOwnerRun(
              [HttpTrigger(AuthorizationLevel.Anonymous , "POST", Route = "SetBeneficialOwner/{accountnumber}/{ownername}")]HttpRequestMessage req,
              string accountnumber,
              string ownername,
              [EventStream("Bank", "Account", "{accountnumber}")]  EventStream bankAccountEvents)
        {

            // Set the start time for how long it took to process the message
            DateTime startTime = DateTime.UtcNow;

            #region Tracing telemetry
            Activity.Current.AddTag("Account Number", accountnumber);
            #endregion

            if (await bankAccountEvents.Exists())
            {
                if (!string.IsNullOrEmpty(ownername))
                {
                    Account.Events.BeneficiarySet evtBeneficiary = new Account.Events.BeneficiarySet()
                    { BeneficiaryName = ownername };
                    await bankAccountEvents.AppendEvent(evtBeneficiary);
                }

                return req.CreateResponse<FunctionResponse>(System.Net.HttpStatusCode.OK,
                        FunctionResponse.CreateResponse(startTime,
                        false,
                        $"Beneficial owner of account {accountnumber} set"),
                        FunctionResponse.MEDIA_TYPE);

            }
            else
            {
                return req.CreateResponse<FunctionResponse>(System.Net.HttpStatusCode.OK,
                    FunctionResponse.CreateResponse(startTime,
                    true,
                    $"Account {accountnumber} does not exist"),
                    FunctionResponse.MEDIA_TYPE);
            }
        }

        #endregion

        #region Get all account numbers
        [FunctionName("GetAllAccounts")]
        public static async Task<HttpResponseMessage> GetAllAccountsRun(
  [HttpTrigger(AuthorizationLevel.Anonymous , "GET", Route = @"GetAllAccounts/{asOfDate?}")]HttpRequestMessage req,
   string asOfDate,
  [Classification("Bank", "Account", "ALL", nameof(InterestAccruedToday))] Classification clsAllAccounts)
        {

            // Set the start time for how long it took to process the message
            DateTime startTime = DateTime.UtcNow;

            IEnumerable<string> allAccounts = await clsAllAccounts.GetAllInstanceKeys(null);
            System.Text.StringBuilder sbRet = new System.Text.StringBuilder();
            if (null != allAccounts)
            {
                foreach (string accountNumber in allAccounts)
                {
                    if (!(sbRet.Length == 0))
                    {
                        sbRet.Append(",");
                    }
                    sbRet.Append(accountNumber);
                }
            }

            return req.CreateResponse<FunctionResponse>(System.Net.HttpStatusCode.OK,
                    FunctionResponse.CreateResponse(startTime,
                    false,
                    $"Account numbers: {sbRet.ToString()}  "),
                    FunctionResponse.MEDIA_TYPE);
        }
        #endregion

        #region Overdrafts
        /// <summary>
        /// Set a new overdraft limit for the account
        /// </summary>
        [FunctionName("SetOverdraftLimit")]
        public static async Task<HttpResponseMessage> SetOverdraftLimitRun(
          [HttpTrigger(AuthorizationLevel.Anonymous , "POST", Route = @"SetOverdraftLimit/{accountnumber}")]HttpRequestMessage req,
          string accountnumber,
          [EventStream("Bank", "Account", "{accountnumber}")]  EventStream bankAccountEvents,
          [Projection("Bank", "Account", "{accountnumber}", nameof(Balance))] Projection prjBankAccountBalance)
        {

            // Set the start time for how long it took to process the message
            DateTime startTime = DateTime.UtcNow;

            #region Tracing telemetry
            Activity.Current.AddTag("Account Number", accountnumber);
            #endregion

            if (!await bankAccountEvents.Exists())
            {
                // You cannot set an overdraft if the account does not exist
                return req.CreateResponse<ProjectionFunctionResponse>(System.Net.HttpStatusCode.Forbidden ,
                    ProjectionFunctionResponse.CreateResponse(startTime,
                    true,
                    $"Account {accountnumber} does not exist",
                    0));
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
                            return req.CreateResponse<ProjectionFunctionResponse>(System.Net.HttpStatusCode.Forbidden,
                                ProjectionFunctionResponse.CreateResponse(startTime,
                                true,
                                $"Failed to write overdraft limit event {exWrite.Message}",
                                projectedBalance.CurrentSequenceNumber  ),
                                FunctionResponse.MEDIA_TYPE);

                        }

                        return req.CreateResponse<ProjectionFunctionResponse>(System.Net.HttpStatusCode.OK ,
                            ProjectionFunctionResponse.CreateResponse(startTime,
                            false,
                            $"{data.NewOverdraftLimit } set as the new overdraft limit for account {accountnumber}",
                            projectedBalance.CurrentSequenceNumber),
                            FunctionResponse.MEDIA_TYPE);

                    }
                    else
                    {
                        return req.CreateResponse<ProjectionFunctionResponse>(System.Net.HttpStatusCode.Forbidden ,
                            ProjectionFunctionResponse.CreateResponse(startTime,
                            true,
                            $"Account {accountnumber} has an outstanding balance beyond the new limit {data.NewOverdraftLimit} (Current balance: {projectedBalance.CurrentBalance} )",
                            projectedBalance.CurrentSequenceNumber),
                            FunctionResponse.MEDIA_TYPE
                            );

                    }
                }
                else
                {
                    return req.CreateResponse<ProjectionFunctionResponse>(System.Net.HttpStatusCode.Forbidden,
                            ProjectionFunctionResponse.CreateResponse(startTime,
                            true,
                            $"Unable to get current balance for account {accountnumber}",
                            0),
                            FunctionResponse.MEDIA_TYPE 
                            );
                }
            }
        }

        #endregion 

        #region Interest accrual and payment

        /// <summary>
        /// Calculate the accrued interest and post it to the account
        /// </summary>
        [FunctionName("AccrueInterest")]
        public static async Task<HttpResponseMessage> AccrueInterestRun(
          [HttpTrigger(AuthorizationLevel.Anonymous, "POST", Route = @"AccrueInterest/{accountnumber}")]HttpRequestMessage req,
          string accountnumber,
          [EventStream("Bank", "Account", "{accountnumber}")]  EventStream bankAccountEvents,
          [Projection("Bank", "Account", "{accountnumber}", nameof(Balance))] Projection prjBankAccountBalance,
          [Classification("Bank", "Account", "{accountnumber}", nameof(InterestAccruedToday))] Classification clsAccruedToday)
        {
            // Set the start time for how long it took to process the message
            DateTime startTime = DateTime.UtcNow;

            #region Tracing telemetry
            Activity.Current.AddTag("Account Number", accountnumber);
            #endregion

            if (! await bankAccountEvents.Exists())
            {
                // You cannot accrue interest if the account does not exist
                return req.CreateResponse<ProjectionFunctionResponse>(System.Net.HttpStatusCode.Forbidden,
                    ProjectionFunctionResponse.CreateResponse(startTime,
                    true,
                    $"Account {accountnumber} does not exist",
                    0));
            }

            ClassificationResponse isAccrued = await clsAccruedToday.Classify< InterestAccruedToday>();
            if (isAccrued.Result == ClassificationResponse.ClassificationResults.Include )
            {
                // The accrual for today has been performed for this account
                return req.CreateResponse<ProjectionFunctionResponse>(System.Net.HttpStatusCode.Forbidden,
                        ProjectionFunctionResponse.CreateResponse(startTime,
                        true,
                        $"Interest accrual already done on account {accountnumber} today",
                        isAccrued.AsOfSequence  ),
                        FunctionResponse.MEDIA_TYPE
                        );
            }
            

            // get the request body...
            InterestAccrualData data = await req.Content.ReadAsAsync<InterestAccrualData>();

            // Get the current account balance, as at midnight
            Balance projectedBalance = await prjBankAccountBalance.Process<Balance>(DateTime.Today);
            if (null != projectedBalance)
            {

                Account.Events.InterestAccrued evAccrued = new Account.Events.InterestAccrued()
                {
                    Commentary = data.Commentary,
                    AccrualEffectiveDate = DateTime.Today  // set the accrual to midnight today  
                };

                if (projectedBalance.CurrentBalance >= 0)
                {
                    // Using the credit rate
                    evAccrued.AmountAccrued = data.CreditInterestRate * projectedBalance.CurrentBalance;
                }
                else
                {
                    // Use the debit rate
                    evAccrued.AmountAccrued = data.DebitInterestRate * projectedBalance.CurrentBalance;
                }

                try
                { 
                   await bankAccountEvents.AppendEvent(evAccrued, isAccrued.AsOfSequence);
                }
                catch (EventSourcingOnAzureFunctions.Common.EventSourcing.Exceptions.EventStreamWriteException exWrite)
                {
                    return req.CreateResponse<ProjectionFunctionResponse>(System.Net.HttpStatusCode.Forbidden,
                        ProjectionFunctionResponse.CreateResponse(startTime,
                        true,
                        $"Failed to write interest accrual event {exWrite.Message}",
                        projectedBalance.CurrentSequenceNumber),
                        FunctionResponse.MEDIA_TYPE);
                }

                return req.CreateResponse<ProjectionFunctionResponse>(System.Net.HttpStatusCode.OK,
                    ProjectionFunctionResponse.CreateResponse(startTime,
                    false,
                    $"Interest accrued for account {accountnumber} is {evAccrued.AmountAccrued}",
                    projectedBalance.CurrentSequenceNumber),
                    FunctionResponse.MEDIA_TYPE);
            }
            else
            {
                return req.CreateResponse<ProjectionFunctionResponse>(System.Net.HttpStatusCode.Forbidden,
                        ProjectionFunctionResponse.CreateResponse(startTime,
                        true,
                        $"Unable to get current balance for account {accountnumber} for interest accrual",
                        0),
                        FunctionResponse.MEDIA_TYPE
                        );
            }

        }

        /// <summary>
        /// Pay the accrued interest and due to/from the account
        /// </summary>
        [FunctionName("PayInterest")]
        public static async Task<HttpResponseMessage> PayInterestRun(
          [HttpTrigger(AuthorizationLevel.Anonymous , "POST", Route = @"PayInterest/{accountnumber}")]HttpRequestMessage req,
          string accountnumber,
          [EventStream("Bank", "Account", "{accountnumber}")]  EventStream bankAccountEvents,
          [Projection("Bank", "Account", "{accountnumber}", nameof(InterestDue ))] Projection prjInterestDue,
          [Projection("Bank", "Account", "{accountnumber}", nameof(Balance))] Projection prjBankAccountBalance,
          [Projection("Bank", "Account", "{accountnumber}", nameof(OverdraftLimit))] Projection prjBankAccountOverdraft)
        {
            // Set the start time for how long it took to process the message
            DateTime startTime = DateTime.UtcNow;

            #region Tracing telemetry
            Activity.Current.AddTag("Account Number", accountnumber);
            #endregion

            if (!await bankAccountEvents.Exists())
            {
                // You cannot pay interest if the account does not exist
                return req.CreateResponse<ProjectionFunctionResponse>(System.Net.HttpStatusCode.Forbidden,
                    ProjectionFunctionResponse.CreateResponse(startTime,
                    true,
                    $"Account {accountnumber} does not exist",
                    0),
                    FunctionResponse.MEDIA_TYPE);
            }

            // get the interest owed / due as now
            InterestDue interestDue = await prjInterestDue.Process<InterestDue>();
            if (null != interestDue )
            {
                // if the interest due is negative we need to make sure the account has sufficient balance
                if (interestDue.Due < 0.00M )
                {
                    Balance balance = await prjBankAccountBalance.Process<Balance>();
                    if (null != balance )
                    {
                        decimal availableBalance = balance.CurrentBalance;
                        
                        // is there an overdraft?
                        OverdraftLimit overdraft = await prjBankAccountOverdraft.Process<OverdraftLimit>();
                        if (null != overdraft )
                        {
                            availableBalance += overdraft.CurrentOverdraftLimit;
                        }

                        if (availableBalance < interestDue.Due  )
                        {
                            // can't pay the interest
                            return req.CreateResponse<ProjectionFunctionResponse>(System.Net.HttpStatusCode.Forbidden,
                                ProjectionFunctionResponse.CreateResponse(startTime,
                                true,
                                $"Unable to pay interest of {interestDue.Due} as available balance is only {availableBalance}",
                                interestDue.CurrentSequenceNumber ),
                                FunctionResponse.MEDIA_TYPE);
                        }
                    }
                }

                // pay the interest
                decimal amountToPay = decimal.Round(interestDue.Due, 2, MidpointRounding.AwayFromZero);
                if (amountToPay != 0.00M)
                {
                    InterestPaid evInterestPaid = new InterestPaid()
                    {
                        AmountPaid = decimal.Round(interestDue.Due, 2, MidpointRounding.AwayFromZero),
                        Commentary = $"Interest due {interestDue.Due} as at {interestDue.CurrentSequenceNumber}"
                    };
                    await bankAccountEvents.AppendEvent(evInterestPaid);

                    return req.CreateResponse<ProjectionFunctionResponse>(System.Net.HttpStatusCode.OK,
                        ProjectionFunctionResponse.CreateResponse(startTime,
                        false,
                        evInterestPaid.Commentary,
                        interestDue.CurrentSequenceNumber),
                        FunctionResponse.MEDIA_TYPE);
                }
                else
                {
                    return req.CreateResponse<ProjectionFunctionResponse>(System.Net.HttpStatusCode.OK,
                        ProjectionFunctionResponse.CreateResponse(startTime,
                        false,
                        $"No interest due so none was paid out",
                        interestDue.CurrentSequenceNumber),
                        FunctionResponse.MEDIA_TYPE);
                }
            }
            else
            {
                return req.CreateResponse<ProjectionFunctionResponse>(System.Net.HttpStatusCode.Forbidden,
                        ProjectionFunctionResponse.CreateResponse(startTime,
                        true,
                        $"Unable to get interest due for account {accountnumber} for interest payment",
                        0),
                        FunctionResponse.MEDIA_TYPE
                        );
            }
        }

        /// <summary>
        /// Extend the overdraft to cover the interest and due from the account
        /// </summary>
        /// <remarks>
        /// If there is no extrension needed then this command does not do anything
        /// </remarks>
        [FunctionName("ExtendOverdraftForInterest")]
        public static async Task<HttpResponseMessage> ExtendOverdraftForInterestRun(
          [HttpTrigger(AuthorizationLevel.Anonymous , "POST", Route = @"ExtendOverdraftForInterest/{accountnumber}")]HttpRequestMessage req,
          string accountnumber,
          [EventStream("Bank", "Account", "{accountnumber}")]  EventStream bankAccountEvents,
          [Projection("Bank", "Account", "{accountnumber}", nameof(InterestDue))] Projection prjInterestDue,
          [Projection("Bank", "Account", "{accountnumber}", nameof(Balance))] Projection prjBankAccountBalance,
          [Projection("Bank", "Account", "{accountnumber}", nameof(OverdraftLimit))] Projection prjBankAccountOverdraft
          )
        {

            // Set the start time for how long it took to process the message
            DateTime startTime = DateTime.UtcNow;

            // Check the balance is negative
            // get the interest owed / due as now
            InterestDue interestDue = await prjInterestDue.Process<InterestDue>();
            if (null != interestDue)
            {
                // if the interest due is negative we need to make sure the account has sufficient balance
                if (interestDue.Due < 0.00M)
                {
                    Balance balance = await prjBankAccountBalance.Process<Balance>();
                    if (null != balance)
                    {
                        decimal availableBalance = balance.CurrentBalance;

                        // is there an overdraft?
                        OverdraftLimit overdraft = await prjBankAccountOverdraft.Process<OverdraftLimit>();
                        if (null != overdraft)
                        {
                            availableBalance += overdraft.CurrentOverdraftLimit;
                        }

                        if (availableBalance < interestDue.Due)
                        {
                            decimal newOverdraft = overdraft.CurrentOverdraftLimit;
                            decimal extension = 10.00M + Math.Abs(interestDue.Due % 10.00M);
                            OverdraftLimitSet evNewLimit = new OverdraftLimitSet()
                            {
                                OverdraftLimit = newOverdraft + extension,
                                Commentary = $"Overdraft extended to pay interest of {interestDue.Due} ",
                                Unauthorised = true
                            };

                            await bankAccountEvents.AppendEvent(evNewLimit);

                            return req.CreateResponse<ProjectionFunctionResponse>(System.Net.HttpStatusCode.OK,
                                ProjectionFunctionResponse.CreateResponse(startTime,
                                false,
                                $"Extended the overdraft by {extension} for payment of interest {interestDue.Due} for account {accountnumber}",
                                interestDue.CurrentSequenceNumber  ),
                                FunctionResponse.MEDIA_TYPE);
                        }
                    }
                }

                // In not needed
                return req.CreateResponse<ProjectionFunctionResponse>(System.Net.HttpStatusCode.OK,
                ProjectionFunctionResponse.CreateResponse(startTime,
                false,
                $"Extending the overdraft for interest not required for account {accountnumber}",
                interestDue.CurrentSequenceNumber),
                FunctionResponse.MEDIA_TYPE);
            }
            else
            {
                return req.CreateResponse<ProjectionFunctionResponse>(System.Net.HttpStatusCode.Forbidden,
                        ProjectionFunctionResponse.CreateResponse(startTime,
                        true,
                        $"Unable to extend the overdraft for account {accountnumber} for interest payment",
                        0),
                        FunctionResponse.MEDIA_TYPE
                        );
            }


        }




#endregion

    }
}
