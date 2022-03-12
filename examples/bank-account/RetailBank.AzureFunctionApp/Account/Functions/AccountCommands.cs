using Azure.Messaging.EventGrid;
using EventSourcingOnAzureFunctions.Common.Binding;
using EventSourcingOnAzureFunctions.Common.CQRS;
using EventSourcingOnAzureFunctions.Common.EventSourcing;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Azure.WebJobs.Extensions.EventGrid;
using Microsoft.Azure.WebJobs.Extensions.Http;
using RetailBank.AzureFunctionApp.Account.Events;
using RetailBank.AzureFunctionApp.Account.Projections;
using System;
using System.Diagnostics;
using System.Net.Http;
using System.Threading.Tasks;

namespace RetailBank.AzureFunctionApp
{
    /// <summary>
    /// Commands related to the Account entity
    /// </summary>
    public partial class AccountCommands
    {

        public const string COMMAND_STEP_OVERDRAFT = "Set Overdraft For Interest";
        public const string COMMAND_STEP_PAY_INTEREST = "Pay Accrued Interest";

        /// <summary>
        /// Apply accrued interest to an account
        /// </summary>
        /// <param name="accountnumber">
        /// The account number to use for the account to have any accrued interest applied.
        /// </param>
        /// <param name="cmdApplyAccruedInterest">
        /// The command orchestration that will perform the steps in turn
        /// </param>
        /// <remarks>
        /// This is a multi-step command, first -if needed- extend an overdraft to cover the 
        /// accrued interest then apply the interest itself
        /// </remarks>
        [FunctionName(nameof(ApplyAccruedInterestCommand))]
        public static async Task<HttpResponseMessage> ApplyAccruedInterestCommand(
                      [HttpTrigger(AuthorizationLevel.Function, "POST", Route = @"ApplyAccruedInterest/{accountnumber}")]HttpRequestMessage req,
                      string accountnumber,
                      [DurableClient] IDurableOrchestrationClient applyInterestOrchestration)
        {

            #region Tracing telemetry
            Activity.Current.AddTag("Account Number", accountnumber);
            #endregion

            // Set the start time for how long it took to process the message
            DateTime startTime = DateTime.UtcNow;

            // use a durable functions GUID so that the orchestration is replayable

            //Command cmdApplyAccruedInterest
            CommandAttribute commandToRun = new CommandAttribute("Bank", "Apply Accrued Interest");
            string commandId = await applyInterestOrchestration.StartNewAsync(nameof(StartCommand), commandToRun);
            commandToRun = new CommandAttribute("Bank", "Apply Accrued Interest", commandId);
            Command cmdApplyAccruedInterest = new Command(commandToRun);

            // No parameters passed in - but set the as-of date/time so that if this command is 
            // re-executed it does not return a different result
            InstanceParameter paramAsOf = new InstanceParameter(cmdApplyAccruedInterest.AsAttribute(), "As Of Date", startTime);
            await applyInterestOrchestration.StartNewAsync(nameof(SetParametersCommandStep), paramAsOf);

            InstanceParameter paramAccount = new InstanceParameter(cmdApplyAccruedInterest.AsAttribute(), "Account Number", accountnumber);
            await applyInterestOrchestration.StartNewAsync(nameof(SetParametersCommandStep), paramAccount );

            // The rest of the command is performed by a durable functions orchestration
            await applyInterestOrchestration.StartNewAsync(nameof(ApplyAccruedInterestCommandStep), cmdApplyAccruedInterest.AsAttribute());

            // Return that the command has been initiated...
            return req.CreateResponse<FunctionResponse>(System.Net.HttpStatusCode.OK,
                    FunctionResponse.CreateResponse(startTime,
                    false,
                    $"Interest accrual process for { accountnumber} initiated"),
                    FunctionResponse.MEDIA_TYPE);

        }

        [FunctionName(nameof(StartCommand))]
        public static async Task StartCommand(
            [OrchestrationTrigger] IDurableOrchestrationContext context)
        {
            CommandAttribute payload = context.GetInput<CommandAttribute>();
            if (payload != null)
            {

                CommandAttribute cmdToUse = new CommandAttribute(payload.DomainName,
                    payload.CommandName,
                    context.InstanceId);
            }
        }

        /// <summary>
        /// Sets a parameter for the given command
        /// </summary>
        /// <param name="context">
        /// </param>
        [FunctionName(nameof(SetParametersCommandStep))]
        public static async Task SetParametersCommandStep
            ([OrchestrationTrigger] IDurableOrchestrationContext context)
        {
            InstanceParameter payload =  context.GetInput<InstanceParameter>();

            if (payload != null)
            {
                Command cmdApplyAccruedInterest = new Command(payload.DomainName, payload.Name, payload.InstanceKey);
                if (cmdApplyAccruedInterest != null)
                {
                    await cmdApplyAccruedInterest.SetParameter(payload.ParameterName, payload.ParameterValue);
                }
            }
        }


        /// <summary>
        /// Orchestration to apply the accrued interest for one account
        /// </summary>
        /// <param name="context">
        /// </param>
        /// <returns></returns>
        [FunctionName(nameof(ApplyAccruedInterestCommandStep))]
        public static async Task ApplyAccruedInterestCommandStep
            ([OrchestrationTrigger] IDurableOrchestrationContext context)
        {
            CommandAttribute payload =  context.GetInput<CommandAttribute>();

            Command cmdApplyAccruedInterest = null;
            if (payload == null)
            {
               cmdApplyAccruedInterest  = new Command(payload );
            }
            else
            {
                cmdApplyAccruedInterest = new Command(new CommandAttribute("Bank", "Apply Accrued Interest"));
            }


            int overdraftEventSequence = await context.CallActivityAsync<int>(nameof(SetOverdraftForInterestCommandStep),
                cmdApplyAccruedInterest.AsAttribute());

             

            bool success = await context.CallActivityAsync<bool>(nameof(PayInterestCommandStep),
                cmdApplyAccruedInterest.AsAttribute());




        }

        /// <summary>
        /// Sets the overdraft for an account if it is required in order to pay the account interest accrued
        /// </summary>
        /// <param name="setOverdraftContext">
        /// The context of the orchestration for which this command is being run
        /// </param>
        /// <returns>
        /// The event sequence number of the event to add interest, if needed (0 if no interest needed)
        /// </returns>
        [FunctionName(nameof(SetOverdraftForInterestCommandStep))]
        public static async Task<int> SetOverdraftForInterestCommandStep
            ([ActivityTrigger] IDurableActivityContext setOverdraftContext)
        {

            int overdraftSequenceNumber = 0;

            CommandAttribute payload = setOverdraftContext.GetInput<CommandAttribute>();

            Command cmdApplyAccruedInterest = null;
            if (payload == null)
            {
                cmdApplyAccruedInterest = new Command(payload);
            }
            else
            {
                cmdApplyAccruedInterest = new Command(new CommandAttribute("Bank", "Apply Accrued Interest"));
            }

            // Set the next step "setting an overdraft if needed"
            await cmdApplyAccruedInterest.InitiateStep(COMMAND_STEP_OVERDRAFT);

            string accountNumber = (string)(await cmdApplyAccruedInterest.GetParameterValue("Account Number"));
            string result = $"No overdraft extension required";

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

                                await bankAccountEvents.AppendEvent(evNewLimit, overdraft.CurrentSequenceNumber );

                                overdraftSequenceNumber = 1 + overdraft.CurrentSequenceNumber;

                                await cmdApplyAccruedInterest.SetParameter("Overdraft Extension", extension);

                                result = $"{evNewLimit.Commentary} on account {accountNumber }";
                            }
                        }
                    }
                }
            }

            await cmdApplyAccruedInterest.SetParameter("Overdraft Event Sequence Number", overdraftSequenceNumber);

            // mark this step as complete
            await cmdApplyAccruedInterest.StepCompleted(COMMAND_STEP_OVERDRAFT, result);

            return overdraftSequenceNumber;
        }

        /// <summary>
        /// Pay any accrued interest
        /// </summary>

        [FunctionName(nameof(PayInterestCommandStep))]
        public static async Task<bool> PayInterestCommandStep
            ([ActivityTrigger] IDurableActivityContext setOverdraftContext)
        {

            string result = "";

            CommandAttribute payload = setOverdraftContext.GetInput<CommandAttribute>();

            Command cmdApplyAccruedInterest = null;
            if (payload == null)
            {
                cmdApplyAccruedInterest = new Command(payload);
            }
            else
            {
                cmdApplyAccruedInterest = new Command(new CommandAttribute("Bank", "Apply Accrued Interest"));
            }

            string accountNumber = (string)(await cmdApplyAccruedInterest.GetParameterValue("Account Number"));
            int overdraftSequenceNumber = 0;
            
            var overdraftSeqNoParam = await cmdApplyAccruedInterest.GetParameterValue("Overdraft Event Sequence Number");
            if (overdraftSeqNoParam != null)
            {
                overdraftSequenceNumber = (int)(overdraftSeqNoParam);
            }

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

                    if (interestDue.CurrentSequenceNumber > overdraftSequenceNumber )
                    {
                        // May indicate a concurrency issue
                        return false;
                    }

                   
                }

            }

            return true;
        }

    }
}
