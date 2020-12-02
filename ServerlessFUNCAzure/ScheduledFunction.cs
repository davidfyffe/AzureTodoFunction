using System;
using System.Linq;
using Microsoft.Azure.Cosmos.Table;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;


namespace ServerlessFUNCAzure
{
    public static class ScheduledFunction
    {
        [FunctionName("ScheduledFunction")]
        public static async System.Threading.Tasks.Task RunAsync([TimerTrigger("0 */5 * * * *")] TimerInfo myTimer,
            [Table("todos", Connection = "AzureWebJobsStorage")] CloudTable todoTable,
            ILogger log)
        {
            log.LogInformation($"C# Timer trigger function executed at: {DateTime.Now}");



            TableQuery<TodoTableEntity> query = new TableQuery<TodoTableEntity>();
            TableQuerySegment<TodoTableEntity> segment = await todoTable.ExecuteQuerySegmentedAsync(query, null);
            var deleted = 0;
            foreach(var todo in segment)
            {
                if(todo.IsCompleted)
                {
                    await todoTable.ExecuteAsync(TableOperation.Delete(todo));
                    deleted++;
                }
            }

            //alt
            TableQuerySegment<TodoTableEntity> tableSegment = await todoTable.ExecuteQuerySegmentedAsync(query, null);
            tableSegment.Results.Where(r => r.IsCompleted).ToList().ForEach(async r => await todoTable.ExecuteAsync(TableOperation.Delete(r)));



        }
    }
}
