using EventSourcingOnAzureFunctions.Common.Binding;
using EventSourcingOnAzureFunctions.Common.EventSourcing;
using EventSourcingOnAzureFunctions.Common.EventSourcing.Interfaces;
using Microsoft.Azure.Documents.SystemFunctions;
using Microsoft.Azure.EventGrid.Models;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.EventGrid;
using Microsoft.Azure.WebJobs.Extensions.Http;
using System;
using System.Diagnostics;
using System.Net.Http;
using System.Threading.Tasks;

namespace EventSourcingOnAzureFunctions.Common.CQRS.ProjectionHandler.Functions
{
    /// <summary>
    /// Event Grid triggered functions used to run projections for commands and queries
    /// </summary>
    /// <remarks>
    /// There are separate functions for handling projections for commands and queries as,
    /// although they are very similar, we might want to separate them completely.
    /// 
    /// Note that the functions runtime cannot discover functions declared in an imported library
    /// so you will need to add a stub to your domain function app that calls into these
    /// functions.
    /// </remarks>
    public static class ProjectionHandlerFunctions
    {

        private static IProjectionMaps _projectionMap;

        /// <summary>
        /// Force run a projection for the given query instance
        /// </summary>
        /// <param name="req">
        /// The HTTP request
        /// </param>
        /// <param name="queryIdentifier">
        /// The unique identifier of the query instance for which the projection should be run
        /// </param>
        /// <returns>
        /// HTTP code indicating success
        /// </returns>
        [FunctionName(nameof(RunProjectionForQuery))]
        public static async Task<HttpResponseMessage> RunProjectionForQueryCommand(
            [HttpTrigger(AuthorizationLevel.Function, "POST", Route = @"CQRS/RunProjectionForQuery/{queryIdentifier}")] HttpRequestMessage req,
                      string queryIdentifier
            )
        {

            #region Tracing telemetry
            Activity.Current.AddTag("Query Identifier", queryIdentifier);
            #endregion

            // 1 - Read the ProjectionRequestData from the body
            ProjectionRequestedEventGridEventData data = await req.Content.ReadAsAsync<ProjectionRequestedEventGridEventData>();
            if (data != null)
            {
                if (string.IsNullOrWhiteSpace(data.InstanceKey))
                {
                    data.InstanceKey = queryIdentifier;
                }
                await RunProjectionForQuery(data);
                return req.CreateResponse(System.Net.HttpStatusCode.OK);
            }
            else
            {
                // The projection cannot be run if there is no body supplied
                return req.CreateResponse(System.Net.HttpStatusCode.BadRequest,
                    "The projection details were not correctly specified in the request body");
            }
        }

        /// <summary>
        /// A projection has been requested in processing a query.  This
        /// function will run it and attach the result back to the query
        /// event stream when complete
        /// </summary>
        /// <param name="eventGridEvent">
        /// The event grid notification that triggered the request for the
        /// projection to be run
        /// </param>
        [FunctionName(nameof(OnQueryProjectionHandler))]
        public static async Task OnQueryProjectionHandler(
            [EventGridTrigger] EventGridEvent eventGridEvent)
        {
            if (eventGridEvent != null)
            {
                // Get the data from the event that describes what projection is requested
                ProjectionRequestedEventGridEventData projectionRequestData = eventGridEvent.Data as ProjectionRequestedEventGridEventData;
                await ProjectionHandlerFunctions.RunProjectionForQuery(projectionRequestData);
            }
        }

        /// <summary>
        /// A projection has been requested in processing a query.  
        /// This function will run it and attach the result back to the query
        /// event stream when complete.
        /// </summary>
        public static async Task RunProjectionForQuery(ProjectionRequestedEventGridEventData projectionRequestData)
        {
            if (projectionRequestData != null)
            {
                // Process the projection
                Projection projection = new Projection(
                    new ProjectionAttribute(
                        projectionRequestData.ProjectionRequest.ProjectionDomainName,
                        projectionRequestData.ProjectionRequest.ProjectionEntityTypeName,
                        projectionRequestData.ProjectionRequest.ProjectionInstanceKey,
                        projectionRequestData.ProjectionRequest.ProjectionTypeName
                        ));

                if (_projectionMap == null)
                {
                    _projectionMap = ProjectionMaps.CreateDefaultProjectionMaps(); 
                }

                IProjection projectionToRun = _projectionMap.CreateProjectionClass(projectionRequestData.ProjectionRequest.ProjectionTypeName);
                IProjection completedProjection = null;

                if (projectionToRun != null)
                {
                    completedProjection = await projection.Process(projectionToRun,
                        projectionRequestData.ProjectionRequest.AsOfDate);
                }

                // Attach the results back to the query event stream
                Query qrySource = new Query(projectionRequestData.DomainName,
                        projectionRequestData.EntityTypeName,
                        projectionRequestData.InstanceKey);


                if (qrySource != null)
                {
                    int CurrentSequenceNumber = 0;
                    if (completedProjection != null)
                    {
                        CurrentSequenceNumber = completedProjection.CurrentSequenceNumber;
                    }

                    await qrySource.PostProjectionResponse(projectionRequestData.ProjectionRequest.ProjectionDomainName,
                        projectionRequestData.ProjectionRequest.ProjectionEntityTypeName,
                        projectionRequestData.ProjectionRequest.ProjectionInstanceKey,
                        projectionRequestData.ProjectionRequest.ProjectionTypeName,
                        projectionRequestData.ProjectionRequest.AsOfDate,
                        projectionRequestData.ProjectionRequest.CorrelationIdentifier,
                        CurrentSequenceNumber,
                        completedProjection);
                }
            }
        }

        /// <summary>
        /// A projection has been requested in processing a command.  This
        /// function will run it and attach the result back to the command
        /// event stream when complete
        /// </summary>
        /// <param name="eventGridEvent">
        /// The event grid notification that triggered the request for the
        /// projection to be run
        /// </param>
        [FunctionName(nameof(OnCommandProjectionHandler))]
        public static async Task OnCommandProjectionHandler(
            [EventGridTrigger] EventGridEvent eventGridEvent)
        {
            if (eventGridEvent != null)
            {
                // Get the data from the event that describes what projection is requested
                ProjectionRequestedEventGridEventData projectionRequestData = eventGridEvent.Data as ProjectionRequestedEventGridEventData ;
                await ProjectionHandlerFunctions.RunProjectionForCommand(projectionRequestData);
            }
        }

        /// <summary>
        /// Run the requested projection for the command and put the resuklts back on the
        /// event stream for that command
        /// </summary>
        /// <param name="req">
        /// The HTTP request with the projection request details in the body
        /// </param>
        /// <param name="commandIdentifier">
        /// The unique identifier of the command for which this projection should be
        /// run
        /// </param>
        /// <returns>
        /// </returns>
        [FunctionName(nameof(RunProjectionForCommand))]
        public static async Task<HttpResponseMessage> RunProjectionForCommandCommand(
              [HttpTrigger(AuthorizationLevel.Function, "POST", 
            Route = @"CQRS/RunProjectionForCommand/{commandIdentifier}")] HttpRequestMessage req,
              string commandIdentifier
            )
        {

            #region Tracing telemetry
            Activity.Current.AddTag("Command Identifier", commandIdentifier);
            #endregion

            // 1 - Read the ProjectionRequestData from the body to use to initiate the request
            ProjectionRequestedEventGridEventData data = await req.Content.ReadAsAsync<ProjectionRequestedEventGridEventData>();
            if (data != null)
            {
                if (string.IsNullOrWhiteSpace(data.InstanceKey))
                {
                    data.InstanceKey = commandIdentifier;
                }
                await RunProjectionForCommand(data);
                return req.CreateResponse(System.Net.HttpStatusCode.OK);
            }
            else
            {
                // The projection cannot be run if there is no body supplied
                return req.CreateResponse(System.Net.HttpStatusCode.BadRequest,
                    "The projection details were not correctly specified in the request body");
            }
        }


        /// <summary>
        /// A projection has been requested in processing a command.  
        /// This function will run it and attach the result back to the command
        /// event stream when complete.
        /// </summary>
        public static async Task RunProjectionForCommand(ProjectionRequestedEventGridEventData projectionRequestData)
        {
            if (projectionRequestData != null)
            {
                // Process the projection
                Projection projection = new Projection(
                    new ProjectionAttribute(
                        projectionRequestData.ProjectionRequest.ProjectionDomainName,
                        projectionRequestData.ProjectionRequest.ProjectionEntityTypeName,
                        projectionRequestData.ProjectionRequest.ProjectionInstanceKey,
                        projectionRequestData.ProjectionRequest.ProjectionTypeName
                        ));

                if (_projectionMap == null)
                {
                    _projectionMap = ProjectionMaps.CreateDefaultProjectionMaps();
                }

                IProjection projectionToRun = _projectionMap.CreateProjectionClass(projectionRequestData.ProjectionRequest.ProjectionTypeName);
                IProjection completedProjection = null;

                if (projectionToRun != null)
                {
                    completedProjection = await projection.Process(projectionToRun,
                        projectionRequestData.ProjectionRequest.AsOfDate);
                }

                // Attach the results back to the query event stream
                Command cmdSource = new Command(projectionRequestData.DomainName,
                        projectionRequestData.EntityTypeName,
                        projectionRequestData.InstanceKey);


                if (cmdSource != null)
                {
                    int CurrentSequenceNumber = 0;
                    if (completedProjection != null)
                    {
                        CurrentSequenceNumber = completedProjection.CurrentSequenceNumber;
                    }

                    await cmdSource.PostProjectionResponse(projectionRequestData.ProjectionRequest.ProjectionDomainName,
                        projectionRequestData.ProjectionRequest.ProjectionEntityTypeName,
                        projectionRequestData.ProjectionRequest.ProjectionInstanceKey,
                        projectionRequestData.ProjectionRequest.ProjectionTypeName,
                        projectionRequestData.ProjectionRequest.AsOfDate,
                        projectionRequestData.ProjectionRequest.CorrelationIdentifier,
                        CurrentSequenceNumber,
                        completedProjection);
                }
            }
        }


    }
}
