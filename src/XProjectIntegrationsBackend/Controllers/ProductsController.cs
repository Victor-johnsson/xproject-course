using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using XProjectIntegrationsBackend.Interfaces;
using XProjectIntegrationsBackend.Models;

namespace XProjectIntegrationsBackend.Controllers;

[Route("/products")]
[ApiController]
public class ProductsController(ILogger<ProductsController> logger, IPimService pimService)
    : ControllerBase
{
    private readonly ILogger<ProductsController> _logger = logger;
    private readonly IPimService _pimService = pimService;

    [HttpGet]
    public async Task<IActionResult> GetAllProducts()
    {
        try
        {
            var data = await _pimService.GetAllProductsAsync();
            return Ok(data);
        }
        catch (Exception ex)
        {
            _logger.LogError("Error fetching products: {Message}", ex.Message);
            return StatusCode(500, $"Internal Server Error: {ex.Message}");
        }
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetProductById(Guid id)
    {
        try
        {
            var data = await _pimService.GetProductByIdAsync(id);
            return Ok(data);
        }
        catch (Exception ex)
        {
            _logger.LogError("Error fetching Product ID {Id}: {Message}", id, ex.Message);
            return StatusCode(500, $"Internal Server Error: {ex.Message}");
        }
    }

    [HttpPost]
    [Authorize(Policy = "MustBeAdmin")]
    public async Task<IActionResult> CreateProduct([FromBody] Product product)
    {
        if (product == null)
        {
            _logger.LogWarning("Received null product object.");
            return BadRequest("Product data is required.");
        }

        try
        {
            var (success, data, statusCode) = await _pimService.CreateProductAsync(product);

            if (!success)
            {
                return StatusCode(
                    statusCode,
                    new ProblemDetails
                    {
                        Status = statusCode,
                        Title = "Product creation failed",
                        Detail = data,
                    }
                );
            }

            return Ok(data);
        }
        catch (HttpRequestException httpEx)
        {
            _logger.LogError(httpEx, "HTTP request error while creating product.");
            return StatusCode(502, "Bad Gateway: Unable to process request.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while creating product.");
            return StatusCode(
                500,
                new ProblemDetails
                {
                    Status = 500,
                    Title = "Internal Server Error",
                    Detail = "An unexpected error occurred. Please try again later.",
                }
            );
        }
    }

    // [HttpDelete("{id}")]
    // [Authorize(Policy = "MustBeAdmin")]
    // // [Authentica
    // public async Task<IActionResult> DeleteProduct(Guid id)
    // {
    //     try
    //     {
    //         var (success, errorMessage, statusCode) = await _pimService.DeleteProductAsync(id);
    //
    //         if (!success)
    //         {
    //             return StatusCode(statusCode, errorMessage);
    //         }
    //
    //         await _pimService.InvalidateCacheForProductAsync(id);
    //
    //         return NoContent();
    //     }
    //     catch (Exception ex)
    //     {
    //         _logger.LogError("Error deleting Product ID {Id}: {Message}", id, ex.Message);
    //         return StatusCode(500, $"Internal Server Error: {ex.Message}");
    //     }
    // }
}
