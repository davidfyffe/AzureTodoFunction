using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Linq;
//using Microsoft.WindowsAzure.Storage.Table;
using Microsoft.Azure.Cosmos.Table;

namespace ServerlessFUNCAzure
{
    public static class TodoApi
    {
        

        [FunctionName("CreateToDo")]
        public static async Task<IActionResult> CreateTodo(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "todo")] HttpRequest req,
            [Table("todos",Connection = "AzureWebJobsStorage")] IAsyncCollector<TodoTableEntity> todoTable,
            [Queue("todos", Connection ="AzureWebJobsStorage")] IAsyncCollector<Todo> todoQueue,

            ILogger log
            )
        {
            log.LogInformation("Creating a new TODO list item");
            var requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            var input = JsonConvert.DeserializeObject<TodoCreateModel>(requestBody);

            var todo = new Todo() { TaskDescription = input.TaskDescription };
            await todoTable.AddAsync(todo.ToTableEntity());
            await todoQueue.AddAsync(todo);
            return new OkObjectResult(todo);
        }

        [FunctionName("GetTodos")]
        public static async Task<IActionResult> GetTodoAsync(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "todo")] HttpRequest req,
            [Table("todos", Connection = "AzureWebJobsStorage")] CloudTable todoTable,
            ILogger log
            )
        {
            var query = new TableQuery<TodoTableEntity>();
            var segment = await todoTable.ExecuteQuerySegmentedAsync(query, null);
            return new OkObjectResult(segment.Select(Mappings.ToTodo));
        }

        [FunctionName("GetTodoById")]
        public static IActionResult GetTodoById(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "todo/{id}")] HttpRequest req,
            [Table("todos", "TODO", "{id}", Connection = "AzureWebJobsStorage")] TodoTableEntity todos,
            ILogger log, string id
            )
        {
            try
            {
                if (todos == null) return new NotFoundResult(); else return new OkObjectResult(todos.ToTodo());
            }
            catch (Exception e)
            {
                return new NotFoundResult();
            }
        }

        [FunctionName("UpdateTodo")]
        public static async Task<IActionResult> updateTodo(
            [HttpTrigger(AuthorizationLevel.Anonymous, "put", Route = "todo/{id}")] HttpRequest req,
            [Table("todos", Connection = "AzureWebJobsStorage")] CloudTable todoTable,
            ILogger log, string id
            )
        {

            var requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            var updated = JsonConvert.DeserializeObject<TodoUpdateModel>(requestBody);


            var findOperation = TableOperation.Retrieve<TodoTableEntity>("TODO", id);
            var result = await todoTable.ExecuteAsync(findOperation);
            if (result.Result == null) return new NotFoundResult();

            TodoTableEntity result1 = (TodoTableEntity)result.Result;
            result1.IsCompleted = updated.IsCompleted;
            result1.TaskDescription = updated.TaskDescription;

            var repalceOperation = TableOperation.Replace(result1);
            await todoTable.ExecuteAsync(repalceOperation);

            return new OkObjectResult(result1.ToTodo());
        }


        [FunctionName("DeleteTodo")]
        public static async Task<IActionResult> DeleteTodoAsync(
    [HttpTrigger(AuthorizationLevel.Anonymous, "delete", Route = "todo/{id}")] HttpRequest req,
    [Table("todos", Connection = "AzureWebJobsStorage")] CloudTable todoTable,
    ILogger log, string id
    )
        {
            var deleteOperation = TableOperation.Delete(new TableEntity()
            {
                PartitionKey="TODO", RowKey=id, ETag="*"
            });
            try
            {
                await todoTable.ExecuteAsync(deleteOperation);
            } catch (Microsoft.WindowsAzure.Storage.StorageException e) when (e.RequestInformation.HttpStatusCode == 404)
            {
                return new NotFoundResult();
            }
            
            return new OkResult();

        }

    }

}
