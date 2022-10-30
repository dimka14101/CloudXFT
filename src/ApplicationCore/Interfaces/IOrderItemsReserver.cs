using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.eShopWeb.ApplicationCore.Entities.OrderAggregate;
using static Microsoft.eShopWeb.ApplicationCore.Services.OrderItemsReserver;

namespace Microsoft.eShopWeb.ApplicationCore.Interfaces;
public interface IOrderItemsReserver
{
    Task BookItemsAtStorageAsync(Dictionary<string, int> quantities);
    Task BookItemsAtCosmoDBAsync(int basketId, decimal sum, Address address);
    Task SendOrderToWarehouseQueueAsync(int basketId, decimal sum, Address address);
}
