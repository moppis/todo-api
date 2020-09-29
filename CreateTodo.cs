using System.IO;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace se.omegapoint.todo
{
    /// Save a todo item to CosmosDB
    public static class CreateTodo
    {
        [FunctionName("CreateTodo")]
        public static IActionResult Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "todo")] HttpRequest req,
            [CosmosDB(
                // % - are needed to tell the functions to look for the value in appsettings or local.settings.json
                // connections string are fetch by default from settings.
                databaseName: "%DATABASE_NAME%",
                collectionName: "%COLLECTION_NAME%",
                ConnectionStringSetting = "COSMOSDB_CONNECTION_STRING")] out Todo newTodo,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");
            string requestBody = new StreamReader(req.Body).ReadToEnd();
            var data = JsonConvert.DeserializeObject<Todo>(requestBody);

            log.LogInformation($"inserting todo: {data.Id}:{data.Description}");
            newTodo = data;

            return new OkObjectResult(data);
        }
    }
}
