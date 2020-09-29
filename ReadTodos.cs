using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace se.omegapoint.todo
{
    /// Read all items in database
    public static class ReadTodos
    {
        [FunctionName("ReadTodos")]
        public static IActionResult Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "todo")] HttpRequest req,
            [CosmosDB(
                databaseName: "%DATABASE_NAME%",
                collectionName: "%COLLECTION_NAME%",
                ConnectionStringSetting = "COSMOSDB_CONNECTION_STRING"
            )] IEnumerable<dynamic> documents, 
            ILogger log)
        {
            log.LogInformation("Fetching all todos");
            return new OkObjectResult(documents);
        }
    }
}
