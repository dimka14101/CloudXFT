using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Azure.Storage.Blobs;
using System.Text;
using Microsoft.Azure.Cosmos;

namespace eShopFunctions
{
    public static class OrderItemsReserveFunction
    {
        private static string databaseId = "eStore";
        private static string containerId = "Orders"; 

        [FunctionName("DeliveryOrderProcessor")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger OrderItemsReserveFunction processed a request.");

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            dynamic data = JsonConvert.DeserializeObject(requestBody);

            //SAVE TO BLOB
            //string Connection = Environment.GetEnvironmentVariable("AzureWebJobsStorage");
            //string containerName = Environment.GetEnvironmentVariable("ContainerName");
            //Stream myBlob = new MemoryStream(Encoding.UTF8.GetBytes(requestBody));
            //string date = $"Request-{DateTime.Now.ToString("dd-MMMM-yyyy hh:mm:ss")}";
            //var blobClient = new BlobContainerClient(Connection, containerName);
            //var blob = blobClient.GetBlobClient(date);
            //await blob.UploadAsync(myBlob);

            //SAVE TO COSMODB
            CosmosClient cosmosClient = new CosmosClient(EndpointUri, PrimaryKey);
            Database database = await cosmosClient.CreateDatabaseIfNotExistsAsync(databaseId);
            Container container = await database.CreateContainerIfNotExistsAsync(containerId, "/Time");

            var obj = JsonConvert.DeserializeObject(requestBody);
            ItemResponse<object> andersenFamilyResponse = await container.CreateItemAsync<object>(obj);


            string responseMessage =  $"Hello, {requestBody}. This HTTP triggered OrderItemsReserveFunction executed successfully.";

            return new OkObjectResult(responseMessage);
        }


        //[FunctionName("OrderItemReserver")]
        //public static void Run([ServiceBusTrigger("orderqueue", Connection = "ServiceBusQueueConnectionString")] string queueMessage, ILogger log)
        //{
        //    log.LogInformation($"C# ServiceBus queue trigger function processed message: {queueMessage}");

        //    string Connection = Environment.GetEnvironmentVariable("AzureWebJobsStorage");
        //    string containerName = Environment.GetEnvironmentVariable("ContainerName");
        //    Stream myBlob = new MemoryStream(Encoding.UTF8.GetBytes(queueMessage));

        //    var blobClient = new BlobContainerClient(Connection, containerName);
        //    var blob = blobClient.GetBlobClient($"Request-{DateTime.Now}");
        //    blob.UploadAsync(myBlob);

        //}
    }
}
