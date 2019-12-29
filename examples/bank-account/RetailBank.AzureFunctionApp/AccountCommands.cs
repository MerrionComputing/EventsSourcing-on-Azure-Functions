using EventSourcingOnAzureFunctions.Common.Binding;
using EventSourcingOnAzureFunctions.Common.CQRS;
using Microsoft.Azure.EventGrid.Models;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.EventGrid;
using Microsoft.Azure.WebJobs.Extensions.Http;
using RetailBank.AzureFunctionApp.Account.Classifications;
using RetailBank.AzureFunctionApp.Account.Events;
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
                      [Command("Bank", "Apply Accrued Interest", "{accountnumber}")] Command cmdApplyAccruedInterest)
        {

            // Set the start time for how long it took to process the message
            DateTime startTime = DateTime.UtcNow;

            // No parameters passed in - but set the as-of date/time so that if this command is 
            // re-executed it does not return a different result
            await cmdApplyAccruedInterest.SetParameter("As Of Date", startTime);

            // Start the set-overdraft-for-interest command step
            await cmdApplyAccruedInterest.InitiateStep(COMMAND_STEP_OVERDRAFT);
            
            // The rest of the command should flow on from that step completing..(how??)

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
        public static void SetOverdraftForInterestCommandStep
            ([EventGridTrigger]EventGridEvent egStepTriggered
             )
        {

            // Get the parameters from the event grid trigger


        }
    }
}
