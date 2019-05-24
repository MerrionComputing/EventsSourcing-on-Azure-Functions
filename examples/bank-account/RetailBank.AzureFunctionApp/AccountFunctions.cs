using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;

using EventSourcingOnAzureFunctions.Common.Binding;
using EventSourcingOnAzureFunctions.Common.EventSourcing;

using RetailBank.AzureFunctionApp.Account.Projections;
using System;

namespace RetailBank.AzureFunctionApp
{
    public static class AccountFunctions
    {


        [FunctionName("GetBalance")]
        public static async Task<HttpResponseMessage> GetBalanceRun(
          [HttpTrigger(AuthorizationLevel.Function, "GET", Route = "GetBalance/{accountnumber}")]HttpRequestMessage req,
          string accountnumber,
          [Projection("Bank", "Account", "{accountnumber}", nameof(Balance))] Projection prjBankAccountBalance)
        {

            string result = $"No balance found for account {accountnumber}";

            if (null != prjBankAccountBalance)
            {
                Balance projectedBalance = await prjBankAccountBalance.Process<Balance>(); 
                if (null != projectedBalance )
                {
                    result = $"Balance for account {accountnumber} is ${projectedBalance.CurrentBalance} (As at record {projectedBalance.CurrentSequenceNumber}) ";
                }
            }

            return req.CreateResponse(System.Net.HttpStatusCode.OK, result); 
        }
    }
}
