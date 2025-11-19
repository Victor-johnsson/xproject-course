using XProjectIntegrationsBackend.Models;

namespace XProjectIntegrationsBackend.Interfaces
{
    public interface IRedisCacheService
    {
        Task<string?> GetAsync(string key);      
        Task RemoveAsync(string key);
        Task SetAsync(string key, string value, TimeSpan? expiry = null);
        Task<List<Product>> GetAllProductsFromRedisAsync(string cacheKey);
        Task AddToCartAsync(string sessionId, CartItem item);
        Task<List<CartItem>> GetCartAsync(string sessionId);
        Task ClearCartAsync(string sessionId);
        Task RemoveFromCartAsync(string sessionId, string productId);
    }
}