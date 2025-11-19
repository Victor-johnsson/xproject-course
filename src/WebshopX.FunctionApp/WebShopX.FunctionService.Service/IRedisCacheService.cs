using WebShopX.FunctionService.core;

namespace WebShopX.FunctionService.Services
{
    public interface IRedisCacheService
    {
        Task<List<Product>> GetAllProductsFromRedisAsync(string prefix);
        Task<string?> GetAsync(string key);
        Task RemoveAsync(string key);
        Task SetAsync(string key, string value, TimeSpan? expiry = null);
        Task SetAllProductsInRedisAsync(string cacheKey, ICollection<Product> data);
    }
}

