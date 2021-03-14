using EventSourcingOnAzureFunctions.Common.Binding;
using EventSourcingOnAzureFunctions.Common.EventSourcing;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using RetailBank.AzureFunctionApp.Transfer.Projections;
using System.Diagnostics;

namespace RetailBank.AzureFunctionApp.Transfer.Functions
{

    /// <summary>
    /// Queries to execute against the money transfer entity
    /// </summary>
    public sealed class TransferQueries
    {

        // GetTransferState
        [FunctionName("GetTransferState")]
        public static async Task<HttpResponseMessage> GetTransferStateRun(
          [HttpTrigger(AuthorizationLevel.Function, "GET", Route = @"GetTransferState/{transfernumber}")] HttpRequestMessage req,
          string transfernumber,
          [Projection("Bank", "Transfer", "{transfernumber}", nameof(TransferState))] Projection prjTransferState)
        {

            // Set the start time for how long it took to process the message
            DateTime startTime = DateTime.UtcNow;

            #region Tracing telemetry
            Activity.Current.AddTag("Transfer Number", transfernumber);
            #endregion

            string result = $"No fund transfer found with the identifier : {transfernumber}";

            if (null != prjTransferState)
            {
                if (await prjTransferState.Exists())
                {
                    // Run the "Transfer state" projection
                    TransferState state =  await prjTransferState.Process<TransferState >();

                    result = $"Transfer {transfernumber} state is {state.LastStateChange} ({state.AmountDeposited} of {state.AmountOfTransferRequested} transfered) ";
                    return req.CreateResponse<ProjectionFunctionResponse>(System.Net.HttpStatusCode.OK,
                            ProjectionFunctionResponse.CreateResponse(startTime,
                            false,
                            result,
                            state.CurrentSequenceNumber),
                            FunctionResponse.MEDIA_TYPE);

                }
                else
                {
                    // No such transfer request exists
                    result = $"Transfer {transfernumber} is not yet created - cannot retrieve a state for it";
                    return req.CreateResponse<ProjectionFunctionResponse>(System.Net.HttpStatusCode.NotFound,
                        ProjectionFunctionResponse.CreateResponse(startTime,
                        true,
                        result,
                        0),
                        FunctionResponse.MEDIA_TYPE);
                }
            }


            // If we got here no transfer was found
            return req.CreateResponse<ProjectionFunctionResponse>(System.Net.HttpStatusCode.NotFound,
                ProjectionFunctionResponse.CreateResponse(startTime,
                true,
                result,
                0),
                FunctionResponse.MEDIA_TYPE);
        }

    }
}
