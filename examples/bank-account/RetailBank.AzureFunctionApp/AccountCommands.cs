using EventSourcingOnAzureFunctions.Common.Binding;
using EventSourcingOnAzureFunctions.Common.CQRS;
using EventSourcingOnAzureFunctions.Common.EventSourcing;
using Microsoft.Azure.EventGrid.Models;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Azure.WebJobs.Extensions.EventGrid;
using Microsoft.Azure.WebJobs.Extensions.Http;
using RetailBank.AzureFunctionApp.Account.Classifications;
using RetailBank.AzureFunctionApp.Account.Events;
using RetailBank.AzureFunctionApp.Account.Projections;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace RetailBank.AzureFunctionApp
{
    /// <summary>
    /// Commands related to the Account entity
    /// </summary>
    public partial class AccountCommands
    {

        const string COMMAND_STEP_OVERDRAFT = "Set Overdraft For Interest";
        const string COMMAND_STEP_PAY_INTEREST = "Pay Accrued Interest";

        /// <summary>
        /// Apply accrued interest to an account
        /// </summary>
        /// <param name="accountnumber">
        /// The account number to use for the account to have any accrued interest applied.
        /// </param>
        /// <remarks>
        /// This is a multi-step command, first -if needed- extend an overdraft to cover the 
        /// accrued interest then apply the interest itself
        /// </remarks>
        [FunctionName(nameof(ApplyAccruedInterestCommand))]
        public static async Task<HttpResponseMessage> ApplyAccruedInterestCommand(
                      [HttpTrigger(AuthorizationLevel.Function, "POST", Route = @"ApplyAccruedInterest/{accountnumber}")]HttpRequestMessage req,
                      string accountnumber,
                      [Command("Bank", "Apply Accrued Interest")] Command cmdApplyAccruedInterest)
        {

            // Set the start time for how long it took to process the message
            DateTime startTime = DateTime.UtcNow;

            // No parameters passed in - but set the as-of date/time so that if this command is 
            // re-executed it does not return a different result
            await cmdApplyAccruedInterest.SetParameter("As Of Date", startTime);
            await cmdApplyAccruedInterest.SetParameter("Account Number", accountnumber );

            // Start the set-overdraft-for-interest command step
            await cmdApplyAccruedInterest.InitiateStep(COMMAND_STEP_OVERDRAFT);

            // The rest of the command should flow on from that step completing .. somehow..


            // Return that the command has been initiated...
            return req.CreateResponse<FunctionResponse>(System.Net.HttpStatusCode.OK,
                    FunctionResponse.CreateResponse(startTime,
                    false,
                    $"Interest accrual process for { accountnumber} initiated"),
                    FunctionResponse.MEDIA_TYPE);

        }


        /// <summary>
        /// Set any overdraft required if the interest accrued exceeds the current requirement
        /// </summary>
        /// <param name="egStepTriggered">
        /// The event grid event that gets sent to this command step
        /// </param>
        public static async void SetOverdraftForInterestCommandStep
            ([EventGridTrigger]EventGridEvent egStepTriggered
             )
        {

            // Get the parameters from the event grid trigger
            // (Payload is a Command Step Initiated)
            Command cmdApplyAccruedInterest = new Command(egStepTriggered);
            if (null != cmdApplyAccruedInterest)
            {

                string result = $"No overdraft extension required";

                if (cmdApplyAccruedInterest.CommandName == COMMAND_STEP_OVERDRAFT)
                {
                    // Get the parameter for account number
                    string accountNumber = (string)(await cmdApplyAccruedInterest.GetParameterValue("Account Number"));
                    if (!string.IsNullOrWhiteSpace(accountNumber))
                    {
                        // run the "set overdraft limit for interest" function 
                        // 1- Get interest due...
                        Projection prjInterestDue = new Projection(
                            new ProjectionAttribute(
                                "Bank",
                                "Account",
                                accountNumber,
                                nameof(InterestDue)
                                ) 
                            );

                        // get the interest owed / due as now
                        InterestDue interestDue = await prjInterestDue.Process<InterestDue>();
                        if (null != interestDue)
                        {
                            // if the interest due is negative we need to make sure the account has sufficient balance
                            if (interestDue.Due < 0.00M)
                            {
                                Projection prjBankAccountBalance = new Projection(
                                    new ProjectionAttribute(
                                        "Bank",
                                        "Account",
                                        accountNumber,
                                        nameof(InterestDue)
                                        )
                                    );

                                Balance balance = await prjBankAccountBalance.Process<Balance>();
                                if (null != balance)
                                {
                                    decimal availableBalance = balance.CurrentBalance;

                                    // is there an overdraft?
                                    Projection prjBankAccountOverdraft = new Projection(
                                                new ProjectionAttribute(
                                                "Bank",
                                                "Account",
                                                accountNumber,
                                                nameof(OverdraftLimit)
                                                )
                                        );

                                    OverdraftLimit overdraft = await prjBankAccountOverdraft.Process<OverdraftLimit>();
                                    if (null != overdraft)
                                    {
                                        availableBalance += overdraft.CurrentOverdraftLimit;
                                    }

                                    if (availableBalance < interestDue.Due)
                                    {
                                        // Force an overdraft extension
                                        EventStream bankAccountEvents = new EventStream(
                                            new EventStreamAttribute(
                                                "Bank",
                                                "Account",
                                                accountNumber
                                                ) 
                                            );

                                        decimal newOverdraft = overdraft.CurrentOverdraftLimit;
                                        decimal extension = 10.00M + Math.Abs(interestDue.Due % 10.00M);
                                        OverdraftLimitSet evNewLimit = new OverdraftLimitSet()
                                        {
                                            OverdraftLimit = newOverdraft + extension,
                                            Commentary = $"Overdraft extended to pay interest of {interestDue.Due} ",
                                            Unauthorised = true
                                        };

                                        await bankAccountEvents.AppendEvent(evNewLimit);

                                        result = $"{evNewLimit.Commentary} on account {accountNumber }";
                                    }
                                }
                            }
                        }
                    }
                }

                // mark this step as complete
                await cmdApplyAccruedInterest.StepCompleted(COMMAND_STEP_OVERDRAFT, result); 
            }
        }

        /// <summary>
        /// Pay any accrued interest
        /// </summary>
        /// <param name="egStepTriggered">
        /// The event grid event that gets sent to this command step
        /// </param>
        public static async void PayInterestCommandStep
            ([EventGridTrigger]EventGridEvent egStepTriggered)
        {
            // Get the parameters from the event grid trigger
            // (Payload is a Command Step Initiated)
            Command cmdApplyAccruedInterest = new Command(egStepTriggered);
            if (null != cmdApplyAccruedInterest)
            {
                string result = $"No accrued interest paid";

                if (cmdApplyAccruedInterest.CommandName == COMMAND_STEP_PAY_INTEREST)
                {
                    // Get the parameter for account number
                    string accountNumber = (string)(await cmdApplyAccruedInterest.GetParameterValue("Account Number"));
                    if (!string.IsNullOrWhiteSpace(accountNumber))
                    {
                        // 1- Get interest due...
                        Projection prjInterestDue = new Projection(
                            new ProjectionAttribute(
                                "Bank",
                                "Account",
                                accountNumber,
                                nameof(InterestDue)
                                )
                            );

                        // get the interest owed / due as now
                        InterestDue interestDue = await prjInterestDue.Process<InterestDue>();
                        if (null != interestDue)
                        {
                            // pay the interest
                            decimal amountToPay = decimal.Round(interestDue.Due, 2, MidpointRounding.AwayFromZero);
                            if (amountToPay != 0.00M)
                            {
                                EventStream bankAccountEvents = new EventStream(
                                    new EventStreamAttribute(
                                        "Bank",
                                        "Account",
                                        accountNumber
                                        )
                                    );

                                InterestPaid evInterestPaid = new InterestPaid()
                                {
                                    AmountPaid = decimal.Round(interestDue.Due, 2, MidpointRounding.AwayFromZero),
                                    Commentary = $"Interest due {interestDue.Due} as at {interestDue.CurrentSequenceNumber}"
                                };
                                await bankAccountEvents.AppendEvent(evInterestPaid);

                                result = $"Interest of {evInterestPaid.AmountPaid} paid for account {accountNumber} ";
                            }
                        }
                    }
                }

                // mark this step as complete
                await cmdApplyAccruedInterest.StepCompleted(COMMAND_STEP_PAY_INTEREST , result);

            }
        }
    }
}
