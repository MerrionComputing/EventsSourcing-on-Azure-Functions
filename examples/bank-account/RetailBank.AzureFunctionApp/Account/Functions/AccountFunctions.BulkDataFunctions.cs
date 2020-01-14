using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace RetailBank.AzureFunctionApp
{
    /// <summary>
    /// Functions that perform bulk operations from files
    /// </summary>
    public partial class AccountFunctions
    {


        // Bulk load new accounts from the folder bank/onboarding
        [FunctionName(nameof(AllClientOnboarding))]
        [StorageAccount("RetailBank")]
        public static async Task AllClientOnboarding(
            [BlobTrigger("bank/onboarding/client-{name}")] Stream accountsToOnboard,
            [DurableClient]  IDurableOrchestrationClient newAccountsOrchestration)
        {

            var serializer = new JsonSerializer();

            List<AccountOpeningData> allNewAccounts = new List<AccountOpeningData>();

            using (var sr = new StreamReader(accountsToOnboard))
            {
                using (JsonTextReader reader = new JsonTextReader(sr))
                {
                    while (reader.Read())
                    {
                        if (reader.TokenType == JsonToken.StartObject)
                        {
                            // Load each object from the stream and do something with it
                            AccountOpeningData nextRecord = JsonSerializer.Create().Deserialize<AccountOpeningData>(reader);
                            if (null != nextRecord)
                            {
                                allNewAccounts.Add(nextRecord);
                            }
                        }
                    }
                }
            }

            await newAccountsOrchestration.StartNewAsync(nameof(AllClientOnboardingOrchestration), allNewAccounts);
        }


        [FunctionName(nameof(AllClientOnboardingOrchestration))]
        public static async Task AllClientOnboardingOrchestration
        ([OrchestrationTrigger] IDurableOrchestrationContext context)
        {
            IEnumerable<AccountOpeningData> allNewAccounts = context.GetInput<IEnumerable<AccountOpeningData>>();
        
            if (null != allNewAccounts)
            {
                // Fan out to create all the accounts magicaly
                var newAccountTasks = new List<Task>();
                foreach (AccountOpeningData newAccount in allNewAccounts )      
                {
                    Task newAccountTask = context.CallSubOrchestratorAsync<AccountOpeningData>(nameof(SingleClientOnboardingOrchestration), newAccount);
                    newAccountTasks.Add(newAccountTask); 
                }

                // Perform all the account onboardings in parrallel
                await Task.WhenAll(newAccountTasks);
            }
        }

        [FunctionName(nameof(SingleClientOnboardingOrchestration)) ]
        public static async Task SingleClientOnboardingOrchestration(
            [OrchestrationTrigger] IDurableOrchestrationContext createAccountContext
            )
        {
        
            AccountOpeningData payload = createAccountContext.GetInput<AccountOpeningData>();
            
            if (null != payload )
            {
                string newAccountNumber = await createAccountContext.CallActivityAsync<string>(nameof( CreateRandomAccountNumber), null);

                string jsonContent = JsonConvert.SerializeObject(payload);

                var newAccountRequest = new DurableHttpRequest(
                                            HttpMethod.Post,
                                            new Uri($"https://retailbankazurefunctionapp.azurewebsites.net/api/OpenAccount/{newAccountNumber}?code=9IcflcYh904F3Dcbp5IGCPuDrqelVJniQK5Ck1ZcVdacOd3Mx4ShWQ=="),
                                            content: jsonContent );


                newAccountRequest.Headers.Add(@"Content-Type",@"application/json"); 

                DurableHttpResponse restartResponse = await createAccountContext.CallHttpAsync(newAccountRequest);
            }
        }

        /// <summary>
        /// Create a random account number to use for onboarding
        /// </summary>
        [FunctionName(nameof(CreateRandomAccountNumber)) ]
        public static  Task<string> CreateRandomAccountNumber(
            [ActivityTrigger] IDurableActivityContext createRandomAccountNumberContext)
        {
            Random rnd = new Random();
            string datepart = DateTime.UtcNow.ToString("yyMMddhhmm");

            return Task<string>.FromResult($"{Convert.ToChar(rnd.Next(65, 90))}{Convert.ToChar(rnd.Next(65, 90))}-{datepart}-{Convert.ToChar(rnd.Next(65, 90))}{Convert.ToChar(rnd.Next(65, 90))}");
        }
    }
}
