using System;
using System.IO;
using System.Net.Http;
using System.Text;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;
using Microsoft.WindowsAzure.Storage.Blob;
using Newtonsoft.Json;

namespace eShopFunctions
{
    public class OrderItemReserver
    {
        [FunctionName("OrderItemReserver")]
        public void Run([ServiceBusTrigger("orderqueue", Connection = "ServiceBusQueueConnectionString")]string myQueueItem, ILogger log)
        {
            log.LogInformation($"C# ServiceBus queue trigger function receive for processing message: {myQueueItem}");

            string Connection = Environment.GetEnvironmentVariable("AzureWebJobsStorage");
            string containerName = Environment.GetEnvironmentVariable("ContainerName");
            Stream myBlob = new MemoryStream(Encoding.UTF8.GetBytes(myQueueItem));

            var options = new BlobClientOptions();
            options.Retry.MaxRetries = 3;

            string dateTime = $"Request-{DateTime.Now.ToString("dd-MMMM-yyyy hh:mm:ss")}";
            log.LogInformation($"Trying to upload blob with date: {dateTime} container: {containerName} and conn: {Connection} ");
            try
            {
                var blobClient = new BlobContainerClient(Connection, containerName, options: options);
                var blob = blobClient.GetBlobClient(dateTime);
                blob.UploadAsync(myBlob);
            }
            catch (Exception ex)
            {
                log.LogError($"C# ServiceBus queue trigger function failed with ex: {ex}");
                HttpClient _client = new HttpClient();
                string functionUrl = "";
                 HttpRequestMessage newRequest = new HttpRequestMessage(HttpMethod.Post, functionUrl);
                newRequest.Content = new StringContent(JsonConvert.SerializeObject(myQueueItem), Encoding.UTF8, "application/json");
                HttpResponseMessage response =  _client.Send(newRequest);

                dynamic responseResults =  response.Content.ReadAsStringAsync();
                log.LogWarning($"Meil was sent with result: {responseResults}");
            }
        }
    }
}
