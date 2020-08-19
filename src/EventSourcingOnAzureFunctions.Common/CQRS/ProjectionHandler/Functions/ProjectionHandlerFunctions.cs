using EventSourcingOnAzureFunctions.Common.Binding;
using EventSourcingOnAzureFunctions.Common.EventSourcing;
using EventSourcingOnAzureFunctions.Common.EventSourcing.Interfaces;
using Microsoft.Azure.Documents.SystemFunctions;
using Microsoft.Azure.EventGrid.Models;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.EventGrid;
using System;
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
                        projectionRequestData.ProjectionRequest.DomainName,
                        projectionRequestData.ProjectionRequest.EntityTypeName,
                        projectionRequestData.ProjectionRequest.InstanceKey,
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

                    await qrySource.PostProjectionResponse(projectionRequestData.ProjectionRequest.DomainName,
                        projectionRequestData.ProjectionRequest.EntityTypeName,
                        projectionRequestData.ProjectionRequest.InstanceKey,
                        projectionRequestData.ProjectionRequest.ProjectionTypeName,
                        projectionRequestData.ProjectionRequest.AsOfDate,
                        projectionRequestData.ProjectionRequest.CorrelationIdentifier,
                        CurrentSequenceNumber,
                        completedProjection);
                }
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
                        projectionRequestData.ProjectionRequest.DomainName,
                        projectionRequestData.ProjectionRequest.EntityTypeName,
                        projectionRequestData.ProjectionRequest.InstanceKey,
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

                    await cmdSource.PostProjectionResponse(projectionRequestData.ProjectionRequest.DomainName,
                        projectionRequestData.ProjectionRequest.EntityTypeName,
                        projectionRequestData.ProjectionRequest.InstanceKey,
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
