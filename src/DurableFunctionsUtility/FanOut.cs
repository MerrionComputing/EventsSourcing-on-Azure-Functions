using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;


namespace DurableFunctionsUtility
{
    public static class FanOut
    {


        public static int THROTTLE_BATCH_SIZE = 200;

        /// <summary>
        /// Run a throttled fan out of the named orchestration passing in each of the 
        /// unique keys for it to run over
        /// </summary>
        /// <param name="client">
        /// The durable orchestration instance to run the fan-out over
        /// </param>
        /// <param name="orchestrationName">
        /// The name of the function to run
        /// </param>
        /// <param name="keys">
        /// The unique keys to pass in to the instances of the orchestration
        /// </param>
        /// <returns>
        /// An awaitable task that performs the fan-out
        /// </returns>
        public static async Task ThrottledFanOut(IDurableOrchestrationClient client,
                                                 string orchestrationName,
                                                 IEnumerable<string> keys)
        {
            if (keys != null)
            {
                await keys.ParallelForEachAsync(THROTTLE_BATCH_SIZE, key => {
                    return client.StartNewAsync(orchestrationName, key);
                });
            }

            return;
        }



        public static async Task ParallelForEachAsync<T>(this IEnumerable<T> items, int maxConcurrency, Func<T, Task> action)
        {
            List<Task> tasks;
            if (items is ICollection<T> itemCollection)
            {
                // optimization to reduce the number of memory allocations
                tasks = new List<Task>(itemCollection.Count);
            }
            else
            {
                tasks = new List<Task>();
            }

            using var semaphore = new SemaphoreSlim(maxConcurrency);
            foreach (T item in items)
            {
                tasks.Add(InvokeThrottledAction(item, action, semaphore));
            }

            await Task.WhenAll(tasks);
        }

        static async Task InvokeThrottledAction<T>(T item, Func<T, Task> action, SemaphoreSlim semaphore)
        {
            await semaphore.WaitAsync();
            try
            {
                await action(item);
            }
            finally
            {
                semaphore.Release();
            }
        }
    }
}
