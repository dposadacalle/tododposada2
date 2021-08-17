using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Microsoft.WindowsAzure.Storage.Table;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Threading.Tasks;
using tododposada2.Common.Models;
using tododposada2.Common.Responses;
using tododposada2.Functions.Entities;

namespace tododposada2.Functions.Functions
{
    public static class TodoApi
    {
        /*
             Daniel Posada
             Date: 16/08/2021
             Methodo: POST
             Description: 
                - Create a Todo
         */
        [FunctionName(nameof(CreateTodo))]
        public static async Task<IActionResult> CreateTodo(

            // Inject through HttpRequest
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "todo")] HttpRequest req,
            // Inject through CloudTable 
            [Table("todo", Connection = "AzureWebJobsStorage")] CloudTable todoTable,
            // Inject through ILogger
            ILogger log)
        {
            log.LogInformation("Recieved a new todo.");

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();

            // Deserialize type Object the RequestBody
            Todo todo = JsonConvert.DeserializeObject<Todo>(requestBody);

            if (string.IsNullOrEmpty(todo?.TaskDescription))
            {
                return new BadRequestObjectResult(new Response
                {
                    IsSucess = false,
                    Message = "The request must have a TaskDescription."
                });
            }

            TodoEntity todoEntity = new TodoEntity
            {
                CreatedTime = DateTime.UtcNow,
                ETag = "*",
                IsCompleted = false,
                PartitionKey = "TODO",
                RowKey = Guid.NewGuid().ToString(),
                TaskDescription = todo.TaskDescription
            };

            TableOperation addOperation = TableOperation.Insert(todoEntity);
            await todoTable.ExecuteAsync(addOperation);

            string message = "New todo stored in table.";
            log.LogInformation(message);


            return new OkObjectResult(new Response
            {
                IsSucess = true,
                Message = message,
                Result = todoEntity
            });

        }

        /*
             Daniel Posada
             Date: 16/08/2021
             Methodo: PUT
             Description: 
                - Update the Todo for the Id
         */
        [FunctionName(nameof(UpdateTodo))]
        public static async Task<IActionResult> UpdateTodo(
            [HttpTrigger(AuthorizationLevel.Anonymous, "put", Route = "todo/{id}")] HttpRequest req,
            [Table("todo", Connection = "AzureWebJobsStorage")] CloudTable todoTable,
            string id,
            ILogger log)
        {
            log.LogInformation($"Update for todo {id}, received.");

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            Todo todo = JsonConvert.DeserializeObject<Todo>(requestBody);

            // Validate todo id
            TableOperation findOperation = TableOperation.Retrieve<TodoEntity>("TODO", id);
            TableResult findResult = await todoTable.ExecuteAsync(findOperation);

            if (findResult.Result == null)
            {
                return new BadRequestObjectResult(new Response
                {
                    IsSucess = false,
                    Message = "Todo not found."
                });
            }

            // Update todo
            TodoEntity todoEntity = (TodoEntity)findResult.Result;
            todoEntity.IsCompleted = todo.IsCompleted;
            // If the description task not it is Empty or Null
            if (!string.IsNullOrEmpty(todo.TaskDescription))
            {
                todoEntity.TaskDescription = todo.TaskDescription;
            }

            TableOperation addOperation = TableOperation.Replace(todoEntity);
            await todoTable.ExecuteAsync(addOperation);

            string message = $"Todo: {id} updated in table.";
            log.LogInformation(message);

            return new OkObjectResult(new Response
            {
                IsSucess = true,
                Message = message,
                Result = todoEntity
            });

        }

        /*
             Daniel Posada
             Date: 16/08/2021
             Methodo: GET
             Description: 
                - Get all the todo
         */
        [FunctionName(nameof(GetAllTodos))]
        public static async Task<IActionResult> GetAllTodos(

             // Inject through HttpRequest
             [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "todo")] HttpRequest req,
             // Inject through CloudTable 
             [Table("todo", Connection = "AzureWebJobsStorage")] CloudTable todoTable,
             // Inject through ILogger
             ILogger log)
        {
            log.LogInformation("Get all todos received.");

            TableQuery<TodoEntity> query = new TableQuery<TodoEntity>();
            TableQuerySegment<TodoEntity> todos = await todoTable.ExecuteQuerySegmentedAsync(query, null);

            string message = "Retrieve all todos.";
            log.LogInformation(message);

            return new OkObjectResult(new Response
            {
                IsSucess = true,
                Message = message,
                Result = todos
            });

        }

        /*
             Daniel Posada
             Date: 16/08/2021
             Methodo: GET
             Description: 
                - Get todo by id
         */
        [FunctionName(nameof(GetTodoById))]
        public static IActionResult GetTodoById(

             [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "todo/{id}")] HttpRequest req,
             [Table("todo", "TODO", "{id}", Connection = "AzureWebJobsStorage")] TodoEntity todoEntity,
             string id,
             ILogger log)
        {
            log.LogInformation($"Get todo by id: {id} received.");


            if (todoEntity == null)
            {
                return new BadRequestObjectResult(new Response
                {
                    IsSucess = false,
                    Message = "Todo not found."
                });
            }

            string message = $"Todo {todoEntity.RowKey}, received.";
            log.LogInformation(message);

            return new OkObjectResult(new Response
            {
                IsSucess = true,
                Message = message,
                Result = todoEntity
            });

        }

        /*
             Daniel Posada
             Date: 16/08/2021
             Methodo: DELETE
             Description: 
                - Delete todo by id
         */
        [FunctionName(nameof(DeleteTodoById))]
        public static async Task<IActionResult> DeleteTodoById(

             [HttpTrigger(AuthorizationLevel.Anonymous, "delete", Route = "todo/{id}")] HttpRequest req,
             [Table("todo", "TODO", "{id}", Connection = "AzureWebJobsStorage")] TodoEntity todoEntity,
             [Table("todo", Connection = "AzureWebJobsStorage")] CloudTable todoTable,
             string id,
             ILogger log)
        {
            log.LogInformation($"Delete todo: {id} received.");

            if (todoEntity == null)
            {
                return new BadRequestObjectResult(new Response
                {
                    IsSucess = false,
                    Message = "Todo not found."
                });
            }

            await todoTable.ExecuteAsync(TableOperation.Delete(todoEntity));

            string message = $"Todo {todoEntity.RowKey}, deleted.";
            log.LogInformation(message);

            return new OkObjectResult(new Response
            {
                IsSucess = true,
                Message = message,
                Result = todoEntity
            });

        }

    }

}
