using EventSourcingOnAzureFunctions.Common.EventSourcing.Implementation.AzureStorage.AppendBlob;
using EventSourcingOnAzureFunctions.Common.EventSourcing.Implementation.AzureStorage.Table;
using EventSourcingOnAzureFunctions.Common.EventSourcing.Implementation.AzureStorage.File;
using EventSourcingOnAzureFunctions.Common.EventSourcing.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EventSourcingOnAzureFunctions.Common.EventSourcing.Implementation
{
    public sealed class ClassificationProcessor
        : IClassificationProcessor
    {

        private readonly IEventStreamReader eventStreamReader = null;


        public async Task<ClassificationResponse> Classify(IClassification classificationToRun, DateTime? asOfDate = null)
        {
            ClassificationResponse.ClassificationResults ret = ClassificationResponse.ClassificationResults.Unchanged;

            bool wasEverIncluded = false;
            bool wasEverExcluded = false;

            if (null != eventStreamReader)
            {
                foreach (IEventContext wrappedEvent in await eventStreamReader.GetEventsWithContext(effectiveDateTime: asOfDate))
                {

                    classificationToRun.OnEventRead(wrappedEvent.SequenceNumber, null);


                    if (classificationToRun.HandlesEventType(wrappedEvent.EventInstance.EventTypeName))
                    {
                        var stepResult = classificationToRun.HandleEvent(wrappedEvent.EventInstance.EventTypeName, wrappedEvent.EventInstance.EventPayload);
                        if (stepResult != ClassificationResponse.ClassificationResults.Unchanged)
                        {
                            // The classification state changed so store it as the current result
                            ret = stepResult;
                            if (ret == ClassificationResponse.ClassificationResults.Include)
                            {
                                wasEverIncluded = true;
                            }
                            if (ret == ClassificationResponse.ClassificationResults.Exclude)
                            {
                                wasEverExcluded = true;
                            }
                        }
                    }

                    // mark the event as handled
                    classificationToRun.MarkEventHandled(wrappedEvent.SequenceNumber);
                }

            }
                return new ClassificationResponse(ret,
                    classificationToRun.CurrentSequenceNumber,
                    classificationToRun.CurrentAsOfDate,
                    wasEverIncluded,
                    wasEverExcluded,
                    Parameters);

        }

        public async Task<ClassificationResponse> Classify<TClassification>(DateTime? asOfDate = null) where TClassification : IClassification, new()
        {
            TClassification classificationToRun = new TClassification();
            return await Classify(classificationToRun, asOfDate); 
        }

        /// <summary>
        /// Does the underlying event stream over which this classification should run exist yet?
        /// </summary>
        public async Task<bool> Exists()
        {
            if (null != eventStreamReader)
            {
                return await eventStreamReader.Exists();
            }
            return false;
        }


        public async Task<IEnumerable<string>> GetAllInstanceKeys(DateTime? asOfDate)
        {
            if (null != eventStreamReader)
            {
                return await eventStreamReader.GetAllInstanceKeys(asOfDate);
            }
            else
            {
                return Enumerable.Empty<string>();
            }
        }


        private readonly Dictionary<string, object> _parameters = new Dictionary<string, object>();

        /// <summary>
        /// The set of name-value parameters set fot the classification
        /// </summary>
        public Dictionary<string, object> Parameters
        {
            get
            {
                return _parameters;
            }
        }

        public void SetParameter(string parameterName, object parameterValue)
        {
            if (!_parameters.ContainsKey(parameterName))
            {
                _parameters.Add(parameterName, parameterValue);
            }
            else
            {
                // Overwrite the previous value
                if (null != parameterValue)
                {
                    _parameters[parameterName] = parameterValue;
                }
                else
                {
                    _parameters[parameterName] = null;
                }
            }
        }

        /// <summary>
        /// Does the named parameter exist
        /// </summary>
        /// <param name="parameterName">
        /// The name of the parameter to find
        /// </param>
        public bool ParameterExists(string parameterName)
        {
            if (_parameters.ContainsKey(parameterName))
            {
                if (null != _parameters[parameterName ] )
                {
                    return true;
                }
            }
            return false;
        }

        public object GetParameterValue(string parameterName)
        {
            if (ParameterExists(parameterName ) )
            {
                return _parameters[parameterName];
            }
            return null;
        }

        /// <summary>
        /// Create a classification processor that works over streams based on an append blob
        /// </summary>
        /// <param name="blobEventStreamReader">
        /// The reader that reads events from an append blob
        /// </param>
        public ClassificationProcessor(BlobEventStreamReader blobEventStreamReader)
        {
            // Initialise the reader to use to read the events to be processed
            this.eventStreamReader = blobEventStreamReader;
        }

        /// <summary>
        /// Create a classification processor that works over streams based on an Azure table
        /// </summary>
        /// <param name="tableEventStreamReader">
        /// The reader that reads events from an Azure table
        /// </param>
        public ClassificationProcessor(TableEventStreamReader tableEventStreamReader)
        {
            // Initialise the reader to use to read the events to be processed
            this.eventStreamReader = tableEventStreamReader;
        }

        /// <summary>
        /// Create a classification processor that works over streams based on Azure files
        /// </summary>
        /// <param name="fileEventStreamReader">
        /// The reader that reads events from Azure files
        /// </param>
        public ClassificationProcessor(FileEventStreamReader fileEventStreamReader)
        {
            // Initialise the reader to use to read the events to be processed
            this.eventStreamReader = fileEventStreamReader;
        }
    }
}
