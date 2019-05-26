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

        /// <summary>
        /// Open a new bank account
        /// </summary>
        /// <param name="accountnumber">
        /// The account number to use for the account.  If this already exists this command will return an error.
        /// </param>
        /// <returns></returns>
        [FunctionName("OpenAccount")]
        public static async Task<HttpResponseMessage> OpenAccountRun(
                      [HttpTrigger(AuthorizationLevel.Function, "POST", Route = "OpenAccount/{accountnumber}")]HttpRequestMessage req,
                      string accountnumber,
                      [EventStream("Bank", "Account", "{accountnumber}")]  EventStream bankAccountEvents)
        {
            if (await bankAccountEvents.Exists())
            {
                return req.CreateResponse(System.Net.HttpStatusCode.Forbidden , $"Account {accountnumber} already exists");
            }
            else
            {
                // Get request body
                AccountOpeningData data = await req.Content.ReadAsAsync<AccountOpeningData>();

                // Append a "created" event
                DateTime dateCreated = DateTime.UtcNow;
                Account.Events.Opened evtOpened = new Account.Events.Opened() { LoggedOpeningDate = dateCreated };
                if (! string.IsNullOrWhiteSpace( data.Commentary))
                {
                    evtOpened.Commentary = data.Commentary;
                }
                await bankAccountEvents.AppendEvent(evtOpened);

                // If there is an initial deposit in the account opening data, append a "deposit" event
                if (data.OpeningBalance.HasValue  )
                {
                    Account.Events.MoneyDeposited evtInitialDeposit = new Account.Events.MoneyDeposited() {
                        AmountDeposited = data.OpeningBalance.Value,
                        LoggedDepositDate = dateCreated,
                        Commentary = "Opening deposit"
                    };
                    await bankAccountEvents.AppendEvent(evtInitialDeposit);
                }
                
                // If there is a beneficiary in the account opening data append a "beneficiary set" event
                if (! string.IsNullOrEmpty(data.ClientName ) )
                {
                    Account.Events.BeneficiarySet evtBeneficiary = new Account.Events.BeneficiarySet()
                    {  BeneficiaryName = data.ClientName   };
                    await bankAccountEvents.AppendEvent(evtBeneficiary); 
                }

                return req.CreateResponse(System.Net.HttpStatusCode.Created , $"Account {accountnumber} created");
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
