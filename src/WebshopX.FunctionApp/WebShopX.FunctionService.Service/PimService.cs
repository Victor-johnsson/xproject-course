using System.Text;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using WebShopX.FunctionService.core;

namespace WebShopX.FunctionService.Services
{
    public interface IPimService
    {
        Task<object> UpdateStock(string id, int itemCount);

        // Task UpdatedTime
        Task<ICollection<Product>> ProductsUpdatedLastHour();
    }

    public class PimService : IPimService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<PimService> _logger;

        public PimService(HttpClient httpClient, ILogger<PimService> logger)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<ICollection<Product>> ProductsUpdatedLastHour()
        {
            var request = new HttpRequestMessage(HttpMethod.Get, $"api/products/updated-last-hour");
            // request.Headers.Add("Ocp-Apim-Subscription-Key", _apimKey);

            var authHeader = Environment.GetEnvironmentVariable("AuthorizationToken");
            if (!string.IsNullOrEmpty(authHeader))
            {
                request.Headers.Add("Authorization", authHeader);
            }

            var response = await _httpClient.SendAsync(request);
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError($"API request failed with status: {response.StatusCode}");
                throw new Exception($"API request failed with status: {response.StatusCode}");
            }

            var data = await response.Content.ReadAsStringAsync();
            var products = JsonConvert.DeserializeObject<List<Product>>(data);

            return products;
        }

        // private async Task LoadSecretsAsync()
        // {
        //     try
        //     {
        //         _logger.LogInformation("Loading secrets from Azure Key Vault...");
        //
        //         KeyVaultSecret? apimUrlSecret =
        //             await _secretClient.GetSecretAsync("PimApimUrl")
        //             ?? throw new InvalidOperationException("PimApimUrl not found in Key Vault.");
        //         KeyVaultSecret? apimKeySecret =
        //             await _secretClient.GetSecretAsync("PimApimSubKey")
        //             ?? throw new InvalidOperationException("PimApimSubKey not found in Key Vault.");
        //
        //         _apimUrl =
        //             apimUrlSecret.Value
        //             ?? throw new InvalidOperationException("PimApimUrl is empty in Key Vault.");
        //         _apimKey =
        //             apimKeySecret.Value
        //             ?? throw new InvalidOperationException("PimApimSubKey is empty in Key Vault.");
        //
        //         _logger.LogInformation("Secrets successfully loaded.");
        //     }
        //     catch (Exception ex)
        //     {
        //         _logger.LogError(ex, "Error loading secrets from Azure Key Vault.");
        //         throw;
        //     }
        // }

        public async Task<object> UpdateStock(string id, int itemCount)
        {
            if (string.IsNullOrWhiteSpace(id) || itemCount < 0)
            {
                _logger.LogWarning(
                    "Invalid parameters. ID: {ProductId}, ItemCount: {ItemCount}",
                    id,
                    itemCount
                );
                return new
                {
                    message = "Invalid input parameters. Ensure itemCount is positive and does not exceed stock.",
                };
            }

            string url = $"api/products/{id}/stock?stockcount={itemCount}";

            _logger.LogInformation(
                "Updating stock for Product ID: {ProductId} at {RequestUrl}",
                id,
                url
            );

            var request = new HttpRequestMessage(HttpMethod.Put, url);
            // request.Headers.Add("Ocp-Apim-Subscription-Key", _apimKey);
            //
            var authHeader = Environment.GetEnvironmentVariable("AuthorizationToken");
            if (!string.IsNullOrEmpty(authHeader))
            {
                request.Headers.Add("Authorization", authHeader);
            }

            var requestBody = new { stockCount = itemCount };
            var jsonBody = JsonConvert.SerializeObject(requestBody);
            var content = new StringContent(jsonBody, Encoding.UTF8, "application/json");
            request.Content = content;

            HttpResponseMessage? response = null;
            try
            {
                _logger.LogInformation(
                    "Sending HTTP request to {RequestUrl} with payload: {Payload}",
                    request.RequestUri,
                    JsonConvert.SerializeObject(requestBody)
                );

                response = await _httpClient.SendAsync(request);
                _logger.LogInformation(
                    "Received response with status code: {StatusCode}",
                    response.StatusCode
                );

                if (!response.IsSuccessStatusCode)
                {
                    var errorResponse = await response.Content.ReadAsStringAsync();
                    return new
                    {
                        message = $"Failed to update stock. Status Code: {response.StatusCode}, Error: {errorResponse}",
                    };
                }

                var responseData = await response.Content.ReadAsStringAsync();
                //var products = JsonConvert.DeserializeObject<List<Product>>(responseData);

                // Try to handle both single object or array
                List<Product> products;
                try
                {
                    products = JsonConvert.DeserializeObject<List<Product>>(responseData);
                }
                catch
                {
                    // Fallback if it's a single object
                    var product = JsonConvert.DeserializeObject<Product>(responseData);
                    products =
                        product != null ? new List<Product> { product } : new List<Product>();
                }

                _logger.LogInformation(
                    "Stock update successful for Product ID: {ProductId}. Response: {ResponseData}",
                    id,
                    responseData
                );

                //return products ?? new List<Product>();
                return products;
            }
            catch (Exception ex)
            {
                return new
                {
                    message = $"Exception occurred: {ex.Message}, StackTrace: {ex.StackTrace}",
                };
            }
            finally
            {
                response?.Dispose();
            }
        }

        //public async Task<object> UpdateStock(string id, int itemCount, int currentStockQuantity)
        //{
        //    if (string.IsNullOrWhiteSpace(id) || itemCount < 0 || itemCount > currentStockQuantity)
        //    {
        //        _logger.LogWarning("Invalid parameters. ID: {ProductId}, ItemCount: {ItemCount}, CurrentStock: {CurrentStock}",
        //                id, itemCount, currentStockQuantity);
        //        return new { message = "Invalid input parameters. Ensure itemCount is positive and does not exceed stock." };
        //    }

        //    var updatedStockQuantity = currentStockQuantity - itemCount;

        //    string url = $"{_apimUrl?.TrimEnd('/')}/{id}/stock?stockcount={updatedStockQuantity}";

        //    _logger.LogInformation("Updating stock for Product ID: {ProductId} at {RequestUrl}", id, url);

        //    var request = new HttpRequestMessage(HttpMethod.Put, url);
        //    request.Headers.Add("Ocp-Apim-Subscription-Key", _apimKey);

        //    var authHeader = Environment.GetEnvironmentVariable("AuthorizationToken");
        //    if (!string.IsNullOrEmpty(authHeader))
        //    {
        //        request.Headers.Add("Authorization", authHeader);
        //    }

        //    var requestBody = new { stockCount = updatedStockQuantity };
        //    var jsonBody = JsonConvert.SerializeObject(requestBody);
        //    var content = new StringContent(jsonBody, Encoding.UTF8, "application/json");
        //    request.Content = content;

        //    HttpResponseMessage? response = null;
        //    try
        //    {
        //        _logger.LogInformation("Sending HTTP request to {RequestUrl} with payload: {Payload}",
        //            request.RequestUri, JsonConvert.SerializeObject(requestBody));

        //        response = await _httpClient.SendAsync(request);
        //        _logger.LogInformation("Received response with status code: {StatusCode}", response.StatusCode);

        //        if (!response.IsSuccessStatusCode)
        //        {
        //            var errorResponse = await response.Content.ReadAsStringAsync();
        //            return new { message = $"Failed to update stock. Status Code: {response.StatusCode}, Error: {errorResponse}" };
        //        }

        //        var responseData = await response.Content.ReadAsStringAsync();
        //        var products = JsonConvert.DeserializeObject<List<Product>>(responseData);

        //        _logger.LogInformation("Stock update successful for Product ID: {ProductId}. Response: {ResponseData}", id, responseData);

        //        return products ?? new List<Product>();
        //    }
        //    catch (Exception ex)
        //    {
        //        return new { message = $"Exception occurred: {ex.Message}, StackTrace: {ex.StackTrace}" };
        //    }
        //    finally
        //    {
        //        response?.Dispose();
        //    }
        //}
    }
}
