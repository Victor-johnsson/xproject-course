using XProjectIntegrationsBackend.Models;

namespace XProjectIntegrationsBackend.Interfaces;

public interface IPimService
{
    Task<ICollection<Product>> GetAllProductsAsync(string? authHeader = null);
    Task<Product> GetProductByIdAsync(Guid id, string? authHeader = null);
    Task<(bool Success, string Data, int StatusCode)> CreateProductAsync(
        Product product,
        string? authHeader = null
    );
    Task<(bool Success, string ErrorMessage, int StatusCode)> DeleteProductAsync(
        Guid id,
        string? authHeader = null
    );
    Task InvalidateCacheForProductAsync(Guid id);
}

