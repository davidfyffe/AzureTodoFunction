using System;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;
using Microsoft.WindowsAzure.Storage.Blob;

namespace ServerlessFUNCAzure
{
    public static class QueueListener
    {
        [FunctionName("QueueListener")]
        public static async System.Threading.Tasks.Task RunAsync([QueueTrigger("todos", Connection = "AzureWebJobsStorage")] Todo todo,
            [Blob("todos", Connection = "AzureWebJobsStorage")] CloudBlobContainer container,

            ILogger log)
        {
            await container.CreateIfNotExistsAsync();
            var blob = container.GetBlockBlobReference($"{todo.Id}.txt");
            await blob.UploadTextAsync($"Created a new task from {todo.TaskDescription}");

            log.LogInformation($"cS queue trigger function processed     {todo.TaskDescription}");
        }
    }
}
