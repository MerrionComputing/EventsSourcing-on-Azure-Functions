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

        /// <summary>
        /// Compare two queries and return the set of differences between them
        /// </summary>
        /// <param name="req">
        /// The HTTP request that triggered this function
        /// </param>
        /// <param name="domainName">
        /// The domain in which these query instances were run
        /// </param>
        /// <param name="queryName">
        /// The name of the type of query that was run
        /// </param>
        /// <param name="sourceQueryIdentifier">
        /// The unique identifier of the first query instance we are comparing
        /// </param>
        /// <param name="targetQueryIdentifier">
        /// The unique identifier of the first query instance we are comparing against
        /// </param>
        /// <remarks>
        /// This is useful to run "what has changed" analysis between two instances of the 
        /// query run at different times.
        /// </remarks>
        /// <returns>
        /// </returns>
        [FunctionName(nameof(CompareQueries))]
        public static async Task<HttpResponseMessage> CompareQueries(
          [HttpTrigger(AuthorizationLevel.Function, "GET", Route = @"CQRS/CompareQueries/{domainName}/{queryName}/{sourceQueryIdentifier}/{targetQueryIdentifier}")] HttpRequestMessage req,
          string domainName,
          string queryName,
          string sourceQueryIdentifier,
          string targetQueryIdentifier
    )
        {

            #region Tracing telemetry
            Activity.Current.AddTag("Domain", domainName);
            Activity.Current.AddTag("Query", queryName);
            Activity.Current.AddTag("Source Query Identifier", sourceQueryIdentifier);
            Activity.Current.AddTag("Traget Query Identifier", targetQueryIdentifier);
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
            if (string.IsNullOrWhiteSpace(sourceQueryIdentifier))
            {
                return req.CreateResponse(System.Net.HttpStatusCode.BadRequest, "Missing Source Query Identifier in query identifier path");
            }
            if (string.IsNullOrWhiteSpace(targetQueryIdentifier))
            {
                return req.CreateResponse(System.Net.HttpStatusCode.BadRequest, "Missing Target Query Identifier in query identifier path");
            }
            #endregion 

            throw new NotImplementedException();
        }

    }
}
