using System;
using System.Collections.Generic;
using Microsoft.Azure.Documents;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;

namespace se.omegapoint.todo
{
    public static class TodoAddedToDB
    {
        [FunctionName("TodoAddedToDB")]
        public static void Run([CosmosDBTrigger(
            databaseName: "%DATABASE_NAME%",
            collectionName: "%COLLECTION_NAME%",
            ConnectionStringSetting = "COSMOSDB_CONNECTION_STRING",
            LeaseCollectionName = "leases",
            CreateLeaseCollectionIfNotExists = true)]IReadOnlyList<Document> input, ILogger log)
        {
            if (input != null && input.Count > 0)
            {
                log.LogInformation("Documents modified " + input.Count);
                log.LogInformation("First document Id " + input[0].Id);
            }
        }
    }
}
