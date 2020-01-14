using EventSourcingOnAzureFunctions.Common.Binding;
using EventSourcingOnAzureFunctions.Common.CQRS;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace RetailBank.AzureFunctionApp
{
    /// <summary>
    /// Queries related to the Account entity
    /// </summary>
    public partial class AccountQueries
    {

        /// <summary>
        /// List all accounts below a given threshold balance..
        /// </summary>
        /// <param name="req">
        /// The HTTP request trigger with the parameters to use in the body as application/json
        /// </param>
        /// <remarks>
        /// This sets up a new [Query], posts the 
        /// </remarks>
        [FunctionName(nameof(AccountsBelowThresholdQuery))]
        public static async Task<HttpResponseMessage> AccountsBelowThresholdQuery(
            [HttpTrigger(AuthorizationLevel.Function, "POST", Route = @"AccountsBelowThresholdQuery")]HttpRequestMessage req,
            [QueryAttribute("Bank","Accounts Below Threshold")] Query qryAccountsBelowThresholdQuery
            )
        {
            // Set the start time for how long it took to process the message
            DateTime startTime = DateTime.UtcNow;

            return req.CreateResponse<FunctionResponse>(System.Net.HttpStatusCode.Forbidden,
                    FunctionResponse.CreateResponse(startTime,
                    true,
                    $"Not implemented"),
                    FunctionResponse.MEDIA_TYPE);
        }

        // Handler for listing the accounts below a given threshold..


    }
}
