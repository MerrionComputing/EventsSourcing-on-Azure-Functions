using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace EventSourcingOnAzureFunctions.Common.CQRS.QueryHandler.Functions
{
    /// <summary>
    /// Functions to interact with query instances 
    /// </summary>
    public static class QueryHandlerFunctions
    {

        // Get query state
        /// <summary>
        /// Get the state of the specified query instance
        /// </summary>
        /// <param name="req">
        /// The HTTP request that triggered this function
        /// </param>
        /// <param name="domainName">
        /// The domain in which this query instance was run
        /// </param>
        /// <param name="queryName">
        /// The name of the type of query that was run
        /// </param>
        /// <param name="queryIdentifier">
        /// The specific instance of the command that was run
        /// </param>
        /// <returns>
        /// A record containing the state of the command as at the point the query was executed
        /// </returns>
        [FunctionName(nameof(GetQueryState))]
        public static async Task<HttpResponseMessage> GetQueryState(
          [HttpTrigger(AuthorizationLevel.Function, "GET", Route = @"CQRS/GetQueryState/{domainName}/{queryName}/{queryIdentifier}")] HttpRequestMessage req,
          string domainName,
          string queryName,
          string queryIdentifier
    )
        {

            #region Tracing telemetry
            Activity.Current.AddTag("Domain", domainName);
            Activity.Current.AddTag("Query", queryName);
            Activity.Current.AddTag("Query Identifier", queryIdentifier);
            #endregion

            #region Validate parameters
            if (string.IsNullOrWhiteSpace(domainName))
            {
                return req.CreateResponse(System.Net.HttpStatusCode.BadRequest, "Missing Domain in query identifier path");
            }
            if (string.IsNullOrWhiteSpace(queryName))
            {
                return req.CreateResponse(System.Net.HttpStatusCode.BadRequest, "Missing Query Name in query identifier path");
            }
            if (string.IsNullOrWhiteSpace(queryIdentifier))
            {
                return req.CreateResponse(System.Net.HttpStatusCode.BadRequest, "Missing Query Identifier in query identifier path");
            }
            #endregion 

            throw new NotImplementedException();
        }

        // Compare Queries

    }
}
