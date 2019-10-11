using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;  

namespace RetailBank.AzureFunctionApp
{
    /// <summary>
    /// A wrapper for the response from the function
    /// </summary>
    public class FunctionResponse
    {

        public const string MEDIA_TYPE = @"application/json";

        /// <summary>
        /// The text message returned from the command
        /// </summary>
        [JsonProperty() ]
        string Message { get; set; }

        /// <summary>
        /// If set the command was not executed because of an error
        /// </summary>
        [JsonProperty()]
        bool InError { get; set; }



        /// <summary>
        /// The amount of time it took for the command to execute
        /// </summary>
        [JsonProperty()]
        TimeSpan ExecutionTime { get; set; }

        /// <summary>
        /// Empty constructor for serialising
        /// </summary>
        public FunctionResponse() { }

        public FunctionResponse(DateTime startTime,
            bool isInError,
            string responseMessage)
        {

            DateTime currentTime = DateTime.UtcNow;

            ExecutionTime = new TimeSpan(currentTime.Ticks - startTime.Ticks);
            InError = isInError;
            Message = responseMessage;
            

        }


        public static FunctionResponse CreateResponse(DateTime startedAt,
            bool isError,
            string message)
        {
            return new FunctionResponse(startedAt,
                isError,
                message);
        }

    }

    public class ProjectionFunctionResponse
        : FunctionResponse 
    {
        /// <summary>
        /// The sequence number as of which the response was sent 
        /// </summary>
        [JsonProperty()]
        int SequenceNumber { get; set; }


        /// <summary>
        /// Empty constructor for serialising
        /// </summary>
        public ProjectionFunctionResponse() { }


        public ProjectionFunctionResponse(DateTime startTime,
            bool isInError,
            string responseMessage,
            int currentSequence)
            : base(startTime,
                  isInError,
                  responseMessage ) 
        {
            SequenceNumber = currentSequence;
        }

        public static ProjectionFunctionResponse CreateResponse(DateTime startedAt,
            bool isError,
            string message,
            int currentSequence)
        {
            return new ProjectionFunctionResponse(startedAt,
                isError,
                message,
                currentSequence);
        }

    }
}
