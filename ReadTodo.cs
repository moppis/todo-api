using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace se.omegapoint.todo
{
    /// Read specific item in database
    public static class ReadTodo
    {
        [FunctionName("ReadTodo")]
        public static IActionResult Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "api/todo/{id}")] HttpRequest req,
            [CosmosDB(
                databaseName: "%DATABASE_NAME%",
                collectionName: "%COLLECTION_NAME%",
                ConnectionStringSetting = "COSMOSDB_CONNECTION_STRING",
                Id = "{id}",
                PartitionKey = "{id}"
            )] Todo todo, 
            ILogger log)
        {
            log.LogInformation($"Fetching todo.");
            if (todo == null) {
                log.LogInformation($"Todo item not found");
                return new NoContentResult();
            }
            
            log.LogInformation($"Found todo: {todo.Id}:{todo.Description}");
            return new OkObjectResult(todo);
        }
    }
}
