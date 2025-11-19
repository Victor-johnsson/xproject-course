using StackExchange.Redis;
using System.Text.Json;
using XProjectIntegrationsBackend.Interfaces;
using XProjectIntegrationsBackend.Models;

namespace XProjectIntegrationsBackend.Services
{
    public class RedisCacheService : IRedisCacheService
    {
        // private readonly IConfiguration _configuration;
        private readonly IConnectionMultiplexer _redis;
        private readonly IDatabase _cacheDb;
        private readonly ILogger _logger;

        public RedisCacheService(IConnectionMultiplexer redis, ILoggerFactory loggerFactory)
        {
            // _configuration = configuration;
            _logger = loggerFactory.CreateLogger<RedisCacheService>();

            _redis = redis ?? throw new ArgumentNullException(nameof(redis));
            _cacheDb = _redis.GetDatabase();
        }

        public async Task SetAsync(string key, string value, TimeSpan? expiry = null)
        {
            int maxRetries = 3;
            int attempt = 0;
            bool success = false;

            while (attempt < maxRetries && !success)
            {
                try
                {
                    await _cacheDb.StringSetAsync(key, value, expiry);
                    success = true;
                }
                catch (RedisConnectionException)
                {
                    attempt++;
                    if (attempt >= maxRetries)
                    {
                        throw;
                    }
                    // You might want to add a small delay between retries
                    await Task.Delay(1000); // Delay for 1 second before retrying
                }
            }
        }

        public async Task<string?> GetAsync(string key)
        {
            try
            {
                var result = await _cacheDb.StringGetAsync(key);
                return result.HasValue ? result.ToString() : null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error retrieving data from Redis: {ex.Message}");
                return null;
            }
        }

        public async Task RemoveAsync(string key)
        {
            await _cacheDb.KeyDeleteAsync(key);
        }

        public async Task<List<Product>> GetAllProductsFromRedisAsync(string cacheKey)
        {
            List<Product> allProducts = new List<Product>();
            int cursor = 0;

            do
            {
                // Execute SCAN command
                var scanResult = (RedisResult[])
                    await _cacheDb.ExecuteAsync(
                        "SCAN",
                        cursor.ToString(),
                        "MATCH",
                        $"{cacheKey}*",
                        "COUNT",
                        "100"
                    );

                // Extract cursor safely
                if (!int.TryParse(scanResult[0].ToString(), out cursor))
                {
                    throw new Exception($"Invalid cursor value from Redis: {scanResult[0]}");
                }

                // Extract keys properly
                var keys = ((RedisResult[])scanResult[1]).Select(x => (RedisValue)x).ToList();

                foreach (var key in keys)
                {
                    var productData = await _cacheDb.StringGetAsync((RedisKey)key.ToString());

                    if (!productData.IsNullOrEmpty)
                    {
                        var product = JsonSerializer.Deserialize<Product>(productData.ToString());
                        allProducts.Add(product);
                    }
                }
            } while (cursor != 0);
            return allProducts;
        }

        public async Task AddToCartAsync(string sessionId, CartItem item)
        {
            string key = $"cart:{sessionId}";

            try
            {
                _logger.LogInformation(
                    "Adding item {ProductId} (Qty: {Quantity}) to cart for session: {SessionId}",
                    item.ProductId,
                    item.Quantity,
                    sessionId
                );

                string cartJson = await _cacheDb.StringGetAsync(key);
                List<CartItem> cartItems = string.IsNullOrEmpty(cartJson)
                    ? new List<CartItem>()
                    : JsonSerializer.Deserialize<List<CartItem>>(cartJson)
                        ?? new List<CartItem>();

                if (cartItems.Count == 0)
                {
                    _logger.LogInformation(
                        "No existing cart found for session: {SessionId}. Creating a new cart.",
                        sessionId
                    );
                }

                var existingItem = cartItems.Find(x => x.ProductId == item.ProductId);
                if (existingItem != null)
                {
                    existingItem.Quantity += item.Quantity;
                    _logger.LogInformation(
                        "Updated quantity for item {ProductId} in cart for session: {SessionId}. New Qty: {Quantity}",
                        item.ProductId,
                        sessionId,
                        existingItem.Quantity
                    );
                }
                else
                {
                    cartItems.Add(item);
                    _logger.LogInformation(
                        "Added new item {ProductId} (Qty: {Quantity}) to cart for session: {SessionId}",
                        item.ProductId,
                        item.Quantity,
                        sessionId
                    );
                }

                // Serialize and update Redis only if changes occurred
                string updatedCartJson = JsonSerializer.Serialize(cartItems);
                if (updatedCartJson != cartJson)
                {
                    await _cacheDb.StringSetAsync(key, updatedCartJson, TimeSpan.FromMinutes(60));
                    _logger.LogInformation(
                        "Cart updated in Redis for session: {SessionId}. Total items: {ItemCount}",
                        sessionId,
                        cartItems.Count
                    );
                }
                else
                {
                    _logger.LogInformation(
                        "No changes detected in cart for session: {SessionId}. Redis update skipped.",
                        sessionId
                    );
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error adding item {ProductId} to cart for session: {SessionId}",
                    item.ProductId,
                    sessionId
                );
                throw;
            }
        }

        public async Task<List<CartItem>> GetCartAsync(string sessionId)
        {
            string key = $"cart:{sessionId}";

            try
            {
                _logger.LogInformation("Fetching cart for session: {SessionId}", sessionId);

                string cartJson = await _cacheDb.StringGetAsync(key);

                if (string.IsNullOrEmpty(cartJson))
                {
                    _logger.LogInformation("No cart found for session: {SessionId}", sessionId);
                    return new List<CartItem>();
                }

                _logger.LogInformation(
                    "Cart retrieved successfully for session: {SessionId}",
                    sessionId
                );
                return JsonSerializer.Deserialize<List<CartItem>>(cartJson)
                    ?? new List<CartItem>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving cart for session: {SessionId}", sessionId);
                return new List<CartItem>();
            }
        }

        public async Task RemoveFromCartAsync(string sessionId, string productId)
        {
            string key = $"cart:{sessionId}";

            try
            {
                _logger.LogInformation(
                    "Attempting to remove item {ProductId} from cart for session: {SessionId}",
                    productId,
                    sessionId
                );

                string cartJson = await _cacheDb.StringGetAsync(key);
                if (string.IsNullOrEmpty(cartJson))
                {
                    _logger.LogInformation(
                        "No cart found for session: {SessionId}. Skipping removal.",
                        sessionId
                    );
                    return;
                }

                var cartItems =
                    JsonSerializer.Deserialize<List<CartItem>>(cartJson) ?? new List<CartItem>();

                var itemToRemove = cartItems.FirstOrDefault(x => x.ProductId == productId);
                if (itemToRemove == null)
                {
                    _logger.LogInformation(
                        "Item {ProductId} not found in cart for session: {SessionId}. No changes made.",
                        productId,
                        sessionId
                    );
                    return;
                }

                cartItems.Remove(itemToRemove);
                _logger.LogInformation(
                    "Item {ProductId} removed from cart for session: {SessionId}",
                    productId,
                    sessionId
                );

                string updatedCartJson = JsonSerializer.Serialize(cartItems);
                await _cacheDb.StringSetAsync(key, updatedCartJson, TimeSpan.FromMinutes(60));
                _logger.LogInformation(
                    "Cart updated in Redis for session: {SessionId}. Remaining items: {ItemCount}",
                    sessionId,
                    cartItems.Count
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error removing item {ProductId} from cart for session: {SessionId}",
                    productId,
                    sessionId
                );
                throw;
            }
        }

        public async Task ClearCartAsync(string sessionId)
        {
            string key = $"cart:{sessionId}";

            try
            {
                bool exists = await _cacheDb.KeyExistsAsync(key);
                if (!exists)
                {
                    _logger.LogInformation("No cart found for session: {SessionId}", sessionId);
                    return;
                }

                await _cacheDb.KeyDeleteAsync(key);
                _logger.LogInformation(
                    "Cart cleared successfully for session: {SessionId}",
                    sessionId
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error clearing cart for session: {SessionId}", sessionId);
                throw;
            }
        }
    }
}
