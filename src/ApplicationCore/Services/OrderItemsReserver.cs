using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Azure.Messaging.ServiceBus;
using Microsoft.Azure.ServiceBus;
using Microsoft.eShopWeb.ApplicationCore.Entities.BasketAggregate;
using Microsoft.eShopWeb.ApplicationCore.Entities.OrderAggregate;
using Microsoft.eShopWeb.ApplicationCore.Interfaces;
using Microsoft.eShopWeb.ApplicationCore.Specifications;
using Microsoft.ServiceBus.Messaging;
using Newtonsoft.Json;

namespace Microsoft.eShopWeb.ApplicationCore.Services;
public class OrderItemsReserver : IOrderItemsReserver
{
    private readonly IAppLogger<BasketService> _logger;
    private readonly IRepository<Basket> _basketRepository;
    public OrderItemsReserver(IRepository<Basket> basketRepository,
                              IAppLogger<BasketService> logger)
    {
        _basketRepository = basketRepository;
        _logger = logger;
    }
    public async Task BookItemsAtStorageAsync(Dictionary<string, int> quantities)
    {
        var order = quantities.Select(x => new { id = x.Key, quantity = x.Value}).ToList();  
        HttpClient _client = new HttpClient();
        //string functionUrl = "http://localhost:7071/api/OrderItemsReserveFunction";
        string functionUrl = "";
        HttpRequestMessage newRequest = new HttpRequestMessage(HttpMethod.Get, functionUrl);
        newRequest.Content = new StringContent(JsonConvert.SerializeObject(order), Encoding.UTF8, "application/json");
        HttpResponseMessage response = await _client.SendAsync(newRequest);

        dynamic responseResults = await response.Content.ReadAsStringAsync();
        _logger.LogWarning($"Order Items Reservedd was calles with result: {responseResults}");
    }

    public async Task BookItemsAtCosmoDBAsync(int basketId, decimal sum, Address address)
    {
        var basketSpec = new BasketWithItemsSpecification(basketId);
        var basket = await _basketRepository.GetBySpecAsync(basketSpec);

        var order = new { id = DateTime.Now.ToString(), items = basket.Items, totalSum = sum, address = address };
        HttpClient _client = new HttpClient();
        //string functionUrl = "http://localhost:7071/api/OrderItemsReserveFunction";
        string functionUrl = "";
        HttpRequestMessage newRequest = new HttpRequestMessage(HttpMethod.Get, functionUrl);
        newRequest.Content = new StringContent(JsonConvert.SerializeObject(order), Encoding.UTF8, "application/json");
        HttpResponseMessage response = await _client.SendAsync(newRequest);

        dynamic responseResults = await response.Content.ReadAsStringAsync();
        _logger.LogWarning($"Order Items Reservedd was calles with result: {responseResults}");
    }

    const string ServiceBusConnectionString = "";
    const string QueueName = "orderqueue";
    public async Task SendOrderToWarehouseQueueAsync(int basketId, decimal sum, Address address)
    {
        var basketSpec = new BasketWithItemsSpecification(basketId);
        var basket = await _basketRepository.GetBySpecAsync(basketSpec);

        var order = new { id = DateTime.Now.ToString(), items = basket.Items, totalSum = sum, address = address };
        HttpClient _client = new HttpClient();
        // string sbQueueUrl = "Endpoint=sb://eshopsb.servicebus.windows.net/;SharedAccessKeyName=eShopSBusQueueKey;SharedAccessKey=jDa1lhwv4C5cVV8mBl/YtJkex+5wJNAJkLfmX1eC/PQ=;EntityPath=orderqueue";

        ServiceBusClient client = new ServiceBusClient(ServiceBusConnectionString);
        // create a sender for the queue.
        ServiceBusSender sender = client.CreateSender(QueueName);
        // create a message that we can send. 
        ServiceBusMessage message = new ServiceBusMessage(JsonConvert.SerializeObject(order));

        // send the message
        await sender.SendMessageAsync(message);
        //_logger.LogWarning($"Order Items Reservedd was calles with result: {responseResults}");
    }
}
