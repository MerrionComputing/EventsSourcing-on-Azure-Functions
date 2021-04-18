using EventSourcingOnAzureFunctions.Common.Binding;
using EventSourcingOnAzureFunctions.Common.CQRS;
using EventSourcingOnAzureFunctions.Common.EventSourcing;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using System;
using System.Diagnostics;
using System.Net.Http;
using System.Threading.Tasks;

namespace RetailBank.AzureFunctionApp.Transfer.Functions
{
    public class TransferCommands
    {

        //Transfer money
        /// <summary>
        /// Initiate a transfer of money between bank accounts
        /// </summary>
        /// <param name="transferidentifier">
        /// (Optional) A unique identifier to use to identify the transfer - if not set a GUID will be supplied
        /// </param>
        [FunctionName(nameof(TransferMoneyCommand))]
        public static async Task<HttpResponseMessage> TransferMoneyCommand(
                      [HttpTrigger(AuthorizationLevel.Function, "POST", Route = @"TransferMoney/{transferidentifier?}")] HttpRequestMessage req,
                      string transferidentifier)
        {

            #region Tracing telemetry
            Activity.Current.AddTag("Transfer Unique Identifier", transferidentifier);
            #endregion

            // Set the start time for how long it took to process the message
            DateTime startTime = DateTime.UtcNow;

            if (string.IsNullOrWhiteSpace(transferidentifier))
            {
                transferidentifier = Guid.NewGuid().ToString("N");
            }

            // Get request body
            MoneyTransferData data = await req.Content.ReadAsAsync<MoneyTransferData>();

            if (data == null)
            {
                return req.CreateResponse<FunctionResponse>(System.Net.HttpStatusCode.BadRequest,
                    FunctionResponse.CreateResponse(startTime,
                    false,
                    $"No transfer request body/data supplied"),
                    FunctionResponse.MEDIA_TYPE);
            }

            if (string.IsNullOrWhiteSpace(data.SourceAccountNumber))
            {
                return req.CreateResponse<FunctionResponse>(System.Net.HttpStatusCode.BadRequest,
                    FunctionResponse.CreateResponse(startTime,
                    false,
                    $"No source account for money transfer"),
                    FunctionResponse.MEDIA_TYPE);
            }

            if (string.IsNullOrWhiteSpace(data.TargetAccountNumber))
            {
                return req.CreateResponse<FunctionResponse>(System.Net.HttpStatusCode.BadRequest,
                    FunctionResponse.CreateResponse(startTime,
                    false,
                    $"No target account for money transfer"),
                    FunctionResponse.MEDIA_TYPE);
            }

            // Create a transfer event stream and add the Transfer Initiated event to it
            Command cmdTransfer = new Command(new CommandAttribute("Bank", "Transfer Money", transferidentifier));

            // set the parameters
            await cmdTransfer.SetParameter(nameof(data.SourceAccountNumber), data.SourceAccountNumber);
            await cmdTransfer.SetParameter(nameof(data.TargetAccountNumber), data.TargetAccountNumber);
            await cmdTransfer.SetParameter(nameof(data.Amount), data.Amount);

            // Start the first step of the command
            await cmdTransfer.InitiateStep("Initiate Transfer");

            // Return that the command has been initiated...
            return req.CreateResponse<FunctionResponse>(System.Net.HttpStatusCode.OK,
                    FunctionResponse.CreateResponse(startTime,
                    false,
                    $"Money transfer initiated - transfer id is {transferidentifier}"),
                    FunctionResponse.MEDIA_TYPE);
        }

        //Cancel in-progress transfer
        [FunctionName(nameof(CancelMoneyTransferCommand))]
        public static async Task<HttpResponseMessage> CancelMoneyTransferCommand(
              [HttpTrigger(AuthorizationLevel.Function, "POST", Route = @"CancelMoneyTransfer/{transferidentifier}")] HttpRequestMessage req,
              string transferidentifier,
              [Command("Bank", "Transfer Money", "{transferidentifier}")] Command cmdTransfer
            )
        {

            #region Tracing telemetry
            Activity.Current.AddTag("Transfer Unique Identifier", transferidentifier);
            #endregion

            // Set the start time for how long it took to process the message
            DateTime startTime = DateTime.UtcNow;

            await cmdTransfer.InitiateStep("Cancel Transfer");

            return req.CreateResponse<FunctionResponse>(System.Net.HttpStatusCode.OK,
                FunctionResponse.CreateResponse(startTime,
                false,
                $"Money transfer cancelation requested - transfer id is {transferidentifier}"),
                FunctionResponse.MEDIA_TYPE);
        }
    }
}
