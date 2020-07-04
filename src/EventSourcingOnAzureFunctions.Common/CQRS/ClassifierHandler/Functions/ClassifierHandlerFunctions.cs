using EventSourcingOnAzureFunctions.Common.Binding;
using EventSourcingOnAzureFunctions.Common.EventSourcing;
using EventSourcingOnAzureFunctions.Common.EventSourcing.Interfaces;
using Microsoft.Azure.EventGrid.Models;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.EventGrid;
using System;
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
    public class ClassifierHandlerFunctions
    {

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
        public static async Task OnQueryClassificationHandler([EventGridTrigger] EventGridEvent eventGridEvent)
        {
            if (eventGridEvent != null)
            {
                // Get the data from the event that describes what classification is requested
                ClassifierRequestedEventGridEventData classifierRequestData = eventGridEvent.Data as ClassifierRequestedEventGridEventData;
                if (classifierRequestData!= null)
                {
                    // handle the classifier request
                    ClassificationResponse response = null;
                    Classification classifier = new Classification(
                        new ClassificationAttribute(
                            classifierRequestData.ClassifierRequest.DomainName,
                            classifierRequestData.ClassifierRequest.EntityTypeName,
                            classifierRequestData.ClassifierRequest.InstanceKey ,
                            classifierRequestData.ClassifierRequest.ClassifierTypeName 
                            ));

                    if (classifier != null)
                    {
                        // get the classifier class - must implement TClassification : IClassification, new()
                        IClassification classificationToRun = Classification.GetClassifierByName(classifier.ClassifierTypeName);
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
        public static Task OnCommandClassificationHandler([EventGridTrigger] EventGridEvent eventGridEvent)
        {

            throw new NotImplementedException();
        }

    }
}
