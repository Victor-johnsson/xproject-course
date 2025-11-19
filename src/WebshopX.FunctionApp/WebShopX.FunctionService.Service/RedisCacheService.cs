using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using StackExchange.Redis;
using WebShopX.FunctionService.core;

namespace WebShopX.FunctionService.Services
{
    public class RedisCacheService : IRedisCacheService
    {
        private readonly IConnectionMultiplexer _redis;
        private readonly IDatabase _cacheDb;
        private readonly ILogger<RedisCacheService> _logger;

        public RedisCacheService(IConnectionMultiplexer redis, ILoggerFactory loggerFactory)
        {
            _redis = redis ?? throw new ArgumentNullException(nameof(redis));
            _cacheDb = _redis.GetDatabase();
            _logger = loggerFactory.CreateLogger<RedisCacheService>();
        }

        public async Task SetAsync(string key, string value, TimeSpan? expiry = null)
        {
            if (string.IsNullOrWhiteSpace(key))
                throw new ArgumentException("Cache key cannot be null or whitespace.", nameof(key));

            int maxRetries = 3;
            for (int attempt = 1; attempt <= maxRetries; attempt++)
            {
                try
                {
                    await _cacheDb.StringSetAsync(key, value, expiry);
                    return;
                }
                catch (RedisConnectionException ex)
                {
                    _logger.LogWarning(ex, "Redis connection failed on attempt {Attempt}", attempt);

                    if (attempt >= maxRetries)
                        throw;

                    await Task.Delay(1000);
                }
            }
        }

        public async Task<string?> GetAsync(string key)
        {
            if (string.IsNullOrWhiteSpace(key))
                return null;

            try
            {
                var result = await _cacheDb.StringGetAsync(key);
                return result.HasValue ? result.ToString() : null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving key '{Key}' from Redis.", key);
                return null;
            }
        }

        public async Task RemoveAsync(string key)
        {
            if (string.IsNullOrWhiteSpace(key))
                return;

            try
            {
                await _cacheDb.KeyDeleteAsync(key);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing key '{Key}' from Redis.", key);
            }
        }

        public async Task<List<Product>> GetAllProductsFromRedisAsync(string prefix)
        {
            if (string.IsNullOrWhiteSpace(prefix))
                throw new ArgumentException("Prefix cannot be null or whitespace.", nameof(prefix));

            var products = new List<Product>();
            long cursor = 0;

            do
            {
                // Safely execute SCAN
                var rawResult = await _cacheDb.ExecuteAsync(
                    "SCAN",
                    cursor.ToString(),
                    "MATCH",
                    $"{prefix}*"
                );

                // Try to cast the *top-level result* into an array
                if (rawResult.Resp2Type != ResultType.Array)
                {
                    _logger.LogWarning("Unexpected SCAN result type: {Type}", rawResult.Resp2Type);
                    break;
                }

                var items = (RedisResult[])rawResult!;

                // Must contain 2 elements: cursor and keys array
                if (items.Length != 2)
                    break;

                // Extract cursor value safely
                cursor = long.TryParse((string)items[0]!, out var next) ? next : 0;

                // Extract keys array
                var keysRaw = (RedisResult[])items[1]!;
                var keys = keysRaw.Select(k => (string)k!).ToArray();

                foreach (var key in keys)
                {
                    var serializedProduct = await _cacheDb.StringGetAsync(key);
                    if (serializedProduct.IsNullOrEmpty)
                        continue;

                    try
                    {
                        var product = JsonConvert.DeserializeObject<Product>(serializedProduct!);
                        if (product != null)
                            products.Add(product);
                    }
                    catch (JsonException ex)
                    {
                        _logger.LogError(ex, "Failed to deserialize product for key {Key}", key);
                    }
                }

            } while (cursor != 0);

            return products;
        }

        public async Task SetAllProductsInRedisAsync(string cacheKey, ICollection<Product> products)
        {
            if (products == null || products.Count == 0)
            {
                _logger.LogWarning("No products available to cache for key '{CacheKey}'.", cacheKey);
                return;
            }

            foreach (var product in products)
            {
                string productCacheKey = $"product:{product.Id}";
                try
                {
                    var serializedProduct = JsonConvert.SerializeObject(product);
                    await SetAsync(productCacheKey, serializedProduct);
                    _logger.LogInformation("Product {ProductId} cached successfully in Redis.", product.Id);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error caching product {ProductId} to Redis.", product.Id);
                }
            }

            _logger.LogInformation("{Count} products cached in Redis under '{CacheKey}'.", products.Count, cacheKey);
        }
    }
}
