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
    /// Update todo item
    public static class UpdateTodo
    {
        [FunctionName("UpdateTodo")]
        public static IActionResult Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "put", Route = "todo")] HttpRequest req,
            [CosmosDB(
                databaseName: "%DATABASE_NAME%",
                collectionName: "%COLLECTION_NAME%",
                ConnectionStringSetting = "COSMOSDB_CONNECTION_STRING",
                Id = "{Query.id}",
                PartitionKey = "{Query.id}"
            )] Todo todo,
            [CosmosDB(
                databaseName: "%DATABASE_NAME%",
                collectionName: "%COLLECTION_NAME%",
                ConnectionStringSetting = "COSMOSDB_CONNECTION_STRING"
            )] out Todo updatedTodo,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");

            string requestBody = new StreamReader(req.Body).ReadToEnd();
            var data = JsonConvert.DeserializeObject<Todo>(requestBody);

            if (data == null)
            {
                log.LogInformation("Todo item not found, creating it.");
                updatedTodo = data;
            }
            else
            {
                log.LogInformation("Todo item found");
                if (data.Id != todo.Id) {
                    log.LogWarning("id's doesn't match!");
                    updatedTodo = todo;
                    return new BadRequestResult();
                }
                log.LogInformation("Updating Todo item");
                updatedTodo = data;
            }
            return new OkObjectResult(updatedTodo);
        }
    }
}
