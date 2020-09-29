using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Microsoft.Azure.Documents.Linq;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace se.omegapoint.todo.functions
{
    public class TodoAPI
    {
        private readonly TelemetryClient telemetryClient;

        public TodoAPI(TelemetryConfiguration configuration)
        {
            this.telemetryClient = new TelemetryClient(configuration);
        }

        [FunctionName("CreateTodo")]
        public IActionResult CreateTodo(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "todo")] HttpRequest req,
            [CosmosDB(
                databaseName: "%DATABASE_NAME%",
                collectionName: "%COLLECTION_NAME%",
                ConnectionStringSetting = "COSMOSDB_CONNECTION_STRING")] out Todo newTodo,
            ILogger log)
        {
            this.telemetryClient.TrackEvent("create-todo");
            
            string requestBody = new StreamReader(req.Body).ReadToEnd();
            var data = JsonConvert.DeserializeObject<Todo>(requestBody);

            log.LogInformation($"inserting todo: {data.Id}:{data.Description}");
            newTodo = data;

            return new OkObjectResult(data);
        }

        [FunctionName("ReadTodo")]
        public IActionResult Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "todo/{id}")] HttpRequest req,
            [CosmosDB(
                databaseName: "%DATABASE_NAME%",
                collectionName: "%COLLECTION_NAME%",
                ConnectionStringSetting = "COSMOSDB_CONNECTION_STRING",
                Id = "{id}",
                PartitionKey = "{id}"
            )] Todo todo, 
            ILogger log)
        {
            this.telemetryClient.TrackEvent("read-todo");

            if (todo == null) {
                log.LogInformation($"Todo item not found");
                return new NoContentResult();
            }
            
            log.LogInformation($"Found todo: {todo.Id}:{todo.Description}");
            
            return new OkObjectResult(todo);
        }

        [FunctionName("ReadTodos")]
        public IActionResult ReadTodos(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "todo")] HttpRequest req,
            [CosmosDB(
                databaseName: "%DATABASE_NAME%",
                collectionName: "%COLLECTION_NAME%",
                ConnectionStringSetting = "COSMOSDB_CONNECTION_STRING"
            )] IEnumerable<dynamic> documents, 
            ILogger log)
        {
            this.telemetryClient.TrackEvent("read-todos");

            return new OkObjectResult(documents);
        }

        [FunctionName("UpdateTodo")]
        public IActionResult UpdateTodo(
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
            this.telemetryClient.TrackEvent("update-todo");

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

        [FunctionName("DeleteTodo")]
        public async Task<IActionResult> DeleteTodo(
            [HttpTrigger(AuthorizationLevel.Anonymous, "delete", Route = "todo/{id}")] HttpRequest req,
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
            this.telemetryClient.TrackEvent("delete-todo");

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

        [FunctionName("TodoAddedToDB")]
        public void TodoAddedToDB([CosmosDBTrigger(
            databaseName: "%DATABASE_NAME%",
            collectionName: "%COLLECTION_NAME%",
            ConnectionStringSetting = "COSMOSDB_CONNECTION_STRING",
            LeaseCollectionName = "leases",
            CreateLeaseCollectionIfNotExists = true)]IReadOnlyList<Document> input, ILogger log)
        {
            this.telemetryClient.TrackEvent("todo-added-to-cosmosdb");
            if (input != null && input.Count > 0)
            {
                log.LogInformation("Documents modified " + input.Count);
                log.LogInformation("First document Id " + input[0].Id);
            }
        }
    }
}
