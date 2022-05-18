using DurableTask.Core;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Azure.WebJobs.Host.Executors;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace EventSourcingOnAzureFunctions.Common.Listener
{
    /// <summary>
    /// A class that caches the azure functions that can be triggered by the various listeners
    /// </summary>
    public abstract class ListenerWorker
    {

        private ITriggeredFunctionExecutor executor = null;


        /// <summary>
        /// Trigger the given azure function
        /// </summary>
        /// <param name="functionData">
        /// The identificantion of the function to be executed
        /// </param>
        /// <exception cref="Exception">
        /// Thrown if execution of the azure function fails
        /// </exception>
        public async Task OnTrigger(TriggeredFunctionData functionData )
        {
            if (executor != null)
            {
                var result = await executor.TryExecuteAsync(functionData, CancellationToken.None);

                if (result.Succeeded == false)
                {
                    throw new Exception(result.Exception.Message, result.Exception);
                }
            }
            else
            {
                throw new ArgumentNullException($"Function executor has not been initialised");
            }
        }

        /// <summary>
        /// Create a new worker class to perform the work of triggering azure functions for a listener
        /// </summary>
        /// <param name="executor">
        /// The triggered function evecutor to use to trigger execution of azure functions
        /// </param>
        public ListenerWorker(ITriggeredFunctionExecutor executor)
        {
            if (executor != null)
            {
                this.executor = executor;
            }
        }
    }
}
