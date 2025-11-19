using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using WebShopX.FunctionService.core;
using WebShopX.FunctionService.Services;

namespace WebShopX.FunctionService.Workers.Functions
{
    public class FetchUpdatedProductsLastHour
    {
        private readonly ILogger<FetchUpdatedProductsLastHour> _logger;

        private readonly IPimService _pimService;
        private readonly IRedisCacheService _cacheService;


        public FetchUpdatedProductsLastHour(
            ILoggerFactory loggerFactory,
            IPimService pimService,
            IRedisCacheService cacheService
        )
        {
            _logger = loggerFactory.CreateLogger<FetchUpdatedProductsLastHour>();
            _pimService = pimService;
            _cacheService = cacheService;
        }


        [Function("FetchUpdatedProductsLastHour")]
        public async Task<IActionResult> Run([TimerTrigger("*/5 * * * * *")] TimerInfo myTimer)
        {
            _logger.LogInformation("Fetching products from the API...");

            try
            {
                var products = await _pimService.ProductsUpdatedLastHour();
                if (products == null || products.Count == 0)
                {
                    _logger.LogWarning("No products were found in the API response.");
                    return new JsonResult(
                        new { message = "No products created or changed during the last hour." }
                    );
                }
                //Store products in Redis
                string cacheKey = "product:";
                await _cacheService.SetAllProductsInRedisAsync(cacheKey, products);

                _logger.LogInformation("Successfully fetched and cached updated products.");
                return new JsonResult(products);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching products.");
                return new StatusCodeResult(StatusCodes.Status500InternalServerError);
            }
        }
    }
}
