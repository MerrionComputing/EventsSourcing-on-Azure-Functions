using EventSourcingOnAzureFunctions.Common.Binding;
using EventSourcingOnAzureFunctions.Common.EventSourcing;
using EventSourcingOnAzureFunctions.Common.EventSourcing.Interfaces;
using Microsoft.Azure.EventGrid.Models;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.EventGrid;
using Microsoft.Azure.WebJobs.Extensions.Http;
using System;
using System.Diagnostics;
using System.Net.Http;
using System.Threading.Tasks;

namespace EventSourcingOnAzureFunctions.Common.CQRS.ClassifierHandler.Functions
{
    /// <summary>
    /// Event Grid triggered functions used to run classifications for commands and queries
    /// </summary>
    /// <remarks>
    /// There are separate functions for handling classifications for commands and queries as,
    /// although they are very similar, we might want to separate them completely
    /// </remarks>
    public static class ClassifierHandlerFunctions
    {

        private static IClassificationMaps _classificationMap;

        /// <summary>
        /// A classification has been requested in processing a query.  This
        /// function will run it and attach the result back to the query
        /// event stream when complete
        /// </summary>
        /// <param name="eventGridEvent">
        /// The event grid notification that triggered the request for the
        /// classification to be run
        /// </param>
        [FunctionName(nameof(OnQueryClassificationHandler))]
        public static async Task OnQueryClassificationHandler(
            [EventGridTrigger] EventGridEvent eventGridEvent)
        {
            if (eventGridEvent != null)
            {
                // Get the data from the event that describes what classification is requested
                ClassifierRequestedEventGridEventData classifierRequestData = eventGridEvent.Data as ClassifierRequestedEventGridEventData;
                await ClassifierHandlerFunctions.RunClassificationForQuery(classifierRequestData);
            }
        }

        /// <summary>
        /// Force run a classification for the given query instance
        /// </summary>
        /// <param name="req"></param>
        /// <param name="queryIdentifier">
        /// The unique identifier of the query instance for which the query should be run
        /// </param>
        /// <returns>
        /// HTTP code indicating success
        /// </returns>
        [FunctionName(nameof(RunClassificationForQuery))]
        public static async Task<HttpResponseMessage> RunClassificationForQueryCommand(
            [HttpTrigger(AuthorizationLevel.Function, "POST", Route = @"CQRS/RunClassificationForQuery/{queryIdentifier}")] HttpRequestMessage req,
                      string queryIdentifier
            )
        {

            #region Tracing telemetry
            Activity.Current.AddTag("Query Identifier", queryIdentifier);
            #endregion

            // 1 - Read the classifierRequestData from the body
            ClassifierRequestedEventGridEventData data = await req.Content.ReadAsAsync<ClassifierRequestedEventGridEventData>();
            if (data != null)
            {
                if (string.IsNullOrWhiteSpace(data.InstanceKey))
                {
                    data.InstanceKey = queryIdentifier;
                }
                await RunClassificationForQuery(data);
            }

            return req.CreateResponse(System.Net.HttpStatusCode.OK);
        }

        /// <summary>
        /// A classification has been requested in processing a query.  
        /// This function will run it and attach the result back to the query
        /// event stream when complete.
        /// </summary>
        public static async Task RunClassificationForQuery(
            ClassifierRequestedEventGridEventData classifierRequestData)
        {
            if (classifierRequestData != null)
            {
                // handle the classifier request
                ClassificationResponse response = null;
                Classification classifier = new Classification(
                    new ClassificationAttribute(
                        classifierRequestData.ClassifierRequest.DomainName,
                        classifierRequestData.ClassifierRequest.EntityTypeName,
                        classifierRequestData.ClassifierRequest.InstanceKey,
                        classifierRequestData.ClassifierRequest.ClassifierTypeName
                        ));

                if (classifier != null)
                {
                    if (_classificationMap == null)
                    {
                        _classificationMap = ClassificationMaps.CreateDefaultClassificationMaps();
                    }
                    // get the classifier class - must implement TClassification : IClassification, new()
                    IClassification classificationToRun = _classificationMap.CreateClassificationClass(classifier.ClassifierTypeName);
                    if (classificationToRun != null)
                    {
                        response = await classifier.Classify(classificationToRun, null);
                    }
                }

                if (response != null)
                {
                    // and post the result back to the query that asked for it
                    Query qrySource = new Query(classifierRequestData.DomainName,
                        classifierRequestData.EntityTypeName,
                        classifierRequestData.InstanceKey);


                    if (qrySource != null)
                    {

                        await qrySource.PostClassifierResponse(classifierRequestData.ClassifierRequest.DomainName,
                             classifierRequestData.ClassifierRequest.EntityTypeName,
                             classifierRequestData.ClassifierRequest.InstanceKey,
                             classifierRequestData.ClassifierRequest.ClassifierTypeName,
                             response.AsOfDate,
                             classifierRequestData.ClassifierRequest.CorrelationIdentifier,
                             response.AsOfSequence,
                             response.Result
                             );

                    }
                }
            }
        }

        /// <summary>
        /// A classification has been requested in processing a command.  
        /// This function will run it and attach the result back to the command
        /// event stream when complete.
        /// </summary>
        public static async Task RunClassificationForCommand(ClassifierRequestedEventGridEventData classifierRequestData)
        {
            if (classifierRequestData != null)
            {
                // handle the classifier request
                ClassificationResponse response = null;
                Classification classifier = new Classification(
                    new ClassificationAttribute(
                        classifierRequestData.ClassifierRequest.DomainName,
                        classifierRequestData.ClassifierRequest.EntityTypeName,
                        classifierRequestData.ClassifierRequest.InstanceKey,
                        classifierRequestData.ClassifierRequest.ClassifierTypeName
                        ));

                if (classifier != null)
                {
                    if (_classificationMap == null)
                    {
                        _classificationMap = ClassificationMaps.CreateDefaultClassificationMaps();
                    }
                    // get the classifier class - must implement TClassification : IClassification, new()
                    IClassification classificationToRun = _classificationMap.CreateClassificationClass(classifier.ClassifierTypeName);
                    if (classificationToRun != null)
                    {
                        response = await classifier.Classify(classificationToRun, null);
                    }
                }

                if (response != null)
                {
                    // and post the result back to the query that asked for it
                    Command cmdSource = new Command(classifierRequestData.DomainName,
                        classifierRequestData.EntityTypeName,
                        classifierRequestData.InstanceKey);


                    if (cmdSource != null)
                    {

                        await cmdSource.PostClassifierResponse(classifierRequestData.ClassifierRequest.DomainName,
                             classifierRequestData.ClassifierRequest.EntityTypeName,
                             classifierRequestData.ClassifierRequest.InstanceKey,
                             classifierRequestData.ClassifierRequest.ClassifierTypeName,
                             response.AsOfDate,
                             classifierRequestData.ClassifierRequest.CorrelationIdentifier,
                             response.AsOfSequence,
                             response.Result
                             );

                    }
                }

            }
        }


        /// <summary>
        /// Force run a classification for the given command instance
        /// </summary>
        /// <param name="req"></param>
        /// <param name="commandIdentifier">
        /// The unique identifier of the command instance for which the query should be run
        /// </param>
        /// <returns>
        /// HTTP code indicating success
        /// </returns>
        [FunctionName(nameof(RunClassificationForCommand))]
        public static async Task<HttpResponseMessage> RunClassificationForCommandCommand(
            [HttpTrigger(AuthorizationLevel.Function, "POST", Route = @"CQRS/RunClassificationForCommand/{commandIdentifier}")] HttpRequestMessage req,
                      string commandIdentifier
            )
        {

            #region Tracing telemetry
            Activity.Current.AddTag("Command Identifier", commandIdentifier);
            #endregion

            // 1 - Read the classifierRequestData from the body
            ClassifierRequestedEventGridEventData data = await req.Content.ReadAsAsync<ClassifierRequestedEventGridEventData>();
            if (data != null)
            {
                if (string.IsNullOrWhiteSpace(data.InstanceKey ))
                {
                    data.InstanceKey = commandIdentifier;
                }
                await RunClassificationForCommand(data);
            }

            return req.CreateResponse(System.Net.HttpStatusCode.OK);
        }

        /// <summary>
        /// A classification has been requested in processing a command.  This
        /// function will run it and attach the result back to the command
        /// event stream when complete
        /// </summary>
        /// <param name="eventGridEvent">
        /// The event grid notification that triggered the request for the
        /// classification to be run
        /// </param>
        [FunctionName(nameof(OnCommandClassificationHandler))]
        public static async Task OnCommandClassificationHandler([EventGridTrigger] EventGridEvent eventGridEvent)
        {

            if (eventGridEvent != null)
            {
                // Get the data from the event that describes what classification is requested
                ClassifierRequestedEventGridEventData classifierRequestData = eventGridEvent.Data as ClassifierRequestedEventGridEventData;

                await ClassifierHandlerFunctions.RunClassificationForCommand(classifierRequestData);
            }
        }



    }
}
