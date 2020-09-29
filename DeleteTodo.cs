using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Microsoft.Azure.Documents.Linq;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace se.omegapoint.todo
{
    public static class DeleteTodo
    {
        [FunctionName("DeleteTodo")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "delete", Route = "api/todo/{id}")] HttpRequest req,
            [CosmosDB(
                databaseName: "%DATABASE_NAME%",
                collectionName: "%COLLECTION_NAME%",
                ConnectionStringSetting = "COSMOSDB_CONNECTION_STRING")] DocumentClient client,
            [CosmosDB(
                databaseName: "%DATABASE_NAME%",
                collectionName: "%COLLECTION_NAME%",
                ConnectionStringSetting = "COSMOSDB_CONNECTION_STRING",
                Id = "{id}",
                PartitionKey = "{id}")] Document document,
            ILogger log)
        {
            if (document == null) {
                log.LogInformation("Todo not found.");
                return new NoContentResult();
            }

            var todo = await client.DeleteDocumentAsync(
                document.SelfLink,  
                new RequestOptions() { 
                    PartitionKey = new PartitionKey(document.Id)
                });

            log.LogInformation($"Todo item deleted");
            return new OkObjectResult(document);
        }
    }
}
