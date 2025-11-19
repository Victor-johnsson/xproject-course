using System.Text;
using System.Text.Json;
using WebShopX.FunctionService.Core.Models;

namespace WebShopX.FunctionService.Services
{
    public interface ICrmService
    {
        Task CreateOrder(Order order);
        Task UpdateStock(string id, int stock);
        Task UpdateOrderStatus(string Status, string OrderNumber);
    }

    public class CrmService : ICrmService
    {
        private readonly HttpClient _httpClient;

        public CrmService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task CreateOrder(Order order)
        {
            string url = "/api/order";
            order.Status = "Pending";
            var json = JsonSerializer.Serialize(order);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var res = await _httpClient.PostAsync(url, content);
        }

        public async Task UpdateStock(string id, int stock)
        {
            string url = $"/api/products/{id}/stock?stockCount={stock}";

            var res = await _httpClient.GetAsync(url);
        }

        public async Task UpdateOrderStatus(string Status, string OrderNumber)
        {
            //TODO: What to send and where
            string url = $"/api/ ???????";

            throw new NotImplementedException();
        }
    }
}
