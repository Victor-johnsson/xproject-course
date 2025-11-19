using System.Text;
using System.Text.Json;
using XProjectIntegrationsBackend.Interfaces;
using XProjectIntegrationsBackend.Models;

namespace XProjectIntegrationsBackend.Services;

public class PimService : IPimService
{
    private readonly ILogger<PimService> _logger;
    private readonly HttpClient _httpClient;

    private readonly IRedisCacheService _cacheService;
    private readonly IImageService _imageService;

    public PimService(
        ILogger<PimService> logger,
        HttpClient httpClient,
        IRedisCacheService cacheService,
        IImageService imageService
    )
    {
        _logger = logger;
        _httpClient = httpClient;
        _cacheService = cacheService;
        _imageService = imageService;
    }

    public async Task<ICollection<Product>> GetAllProductsAsync(string? authHeader = null)
    {
        string cacheKey = "product:";
        try
        {
            // Check if products exist in Redis Cache
            var cachedData = await _cacheService.GetAllProductsFromRedisAsync(cacheKey);
            if (cachedData != null && cachedData.Count > 0)
            {
                _logger.LogInformation("Cache hit for all products");
                return cachedData;
            }

            _logger.LogInformation("Cache miss. Fetching from APIM.");

            // Request to APIM
            var request = new HttpRequestMessage(HttpMethod.Get, "/api/products");
            // request.Headers.Add("Ocp-Apim-Subscription-Key", apimKey);

            // if (!string.IsNullOrEmpty(authHeader))
            // {
            //     request.Headers.Add("Authorization", authHeader);
            // }

            var response = await _httpClient.SendAsync(request);
            if (!response.IsSuccessStatusCode)
            {
                throw new HttpRequestException($"API returned {response.StatusCode}");
            }

            return await response.Content.ReadFromJsonAsync<ICollection<Product>>()!;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching products");
            throw;
        }
    }

    public async Task<Product> GetProductByIdAsync(Guid id, string? authHeader = null)
    {
        string cacheKey = $"product:{id}";

        try
        {
            // Check if product exists in Redis Cache
            var cachedData = await _cacheService.GetAsync(cacheKey);
            if (!string.IsNullOrEmpty(cachedData))
            {
                _logger.LogDebug("Cache hit for Product ID: {id}", id);
                return JsonSerializer.Deserialize<Product>(cachedData);
            }

            _logger.LogInformation("Cache miss. Fetching Product ID: {id} from APIM.", id);

            // Request to APIM
            var request = new HttpRequestMessage(HttpMethod.Get, $"/api/products/{id}");
            // request.Headers.Add("Ocp-Apim-Subscription-Key", apimKey);

            // if (!string.IsNullOrEmpty(authHeader))
            // {
            //     request.Headers.Add("Authorization", authHeader);
            // }

            var response = await _httpClient.SendAsync(request);
            if (!response.IsSuccessStatusCode)
            {
                throw new HttpRequestException($"API returned {response.StatusCode}");
            }

            var data = await response.Content.ReadFromJsonAsync<Product>();

            // Store in cache for future requests
            await _cacheService.SetAsync(cacheKey, await response.Content.ReadAsStringAsync());

            return data;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching Product ID {Id}", id);
            throw;
        }
    }

    public async Task<(bool Success, string Data, int StatusCode)> CreateProductAsync(
        Product product,
        string? authHeader = null
    )
    {
        try
        {
            if (string.IsNullOrEmpty(product.ImageBase64) || string.IsNullOrWhiteSpace(product.Name))
            {
                return (false, "Name or image was not provided", 400);
            }
            else
            {
                string imageUrl = await _imageService.UploadImageAsync(
                    product.ImageBase64,
                    product.Name
                );
                product.ImageUrl = imageUrl;
            }

            var jsonData = JsonSerializer.Serialize(product);
            using var content = new StringContent(jsonData, Encoding.UTF8, "application/json");

            using var request = new HttpRequestMessage(HttpMethod.Post, "/api/products")
            {
                Content = content,
            };
            // request.Headers.Add("Ocp-Apim-Subscription-Key", apimKey);

            // if (!string.IsNullOrEmpty(authHeader))
            // {
            //     request.Headers.Add("Authorization", authHeader);
            // }

            var response = await _httpClient.SendAsync(request);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError($"Failed to create product. Status: {response.StatusCode}");
                return (
                    false,
                    await response.Content.ReadAsStringAsync(),
                    (int)response.StatusCode
                );
            }
            var data = await response.Content.ReadAsStringAsync();
            return (true, data, (int)response.StatusCode);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while creating product.");
            throw;
        }
    }

    public async Task<(bool Success, string ErrorMessage, int StatusCode)> DeleteProductAsync(
        Guid id,
        string? authHeader = null
    )
    {
        try
        {
            var request = new HttpRequestMessage(HttpMethod.Delete, $"api/products/{id}");
            // request.Headers.Add("Ocp-Apim-Subscription-Key", apimKey);

            // if (!string.IsNullOrEmpty(authHeader))
            // {
            //     request.Headers.Add("Authorization", authHeader);
            // }

            var response = await _httpClient.SendAsync(request);
            if (!response.IsSuccessStatusCode)
            {
                return (false, "Failed to delete product", (int)response.StatusCode);
            }

            return (true, string.Empty, (int)response.StatusCode);
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error deleting Product ID {id}: {ex.Message}");
            throw;
        }
    }

}
