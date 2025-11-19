# Exercise 02 – Integrating with the Product Service (PIM Integration)

## Objective

In this exercise, you’ll implement the **`IPimService`** interface to connect to the Product Catalog (PIM) REST API.
This teaches you how to:
- Integrate with external REST services
- Handle authentication headers and HTTP response codes
- Prepare your backend for external service communication (to be used later in Azure)

---

## 1. Background

The Product Catalog system (PIM) is an **external service** already running in your Aspire-hosted environment.
It exposes a REST API with endpoints for managing product data.

Your backend will use this API via an interface abstraction, ensuring your code remains **testable, decoupled**, and **ready for Azure deployment** later.

---

## 2. Interface Definition

You have the following interface defined in your solution (under the intefaces folder for backend project):

```csharp
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
```

Your task is to create a class (e.g. `PimService`) that **implements this interface** using `HttpClient`.

---

## 3. Implementation Tasks

### Step 1 – Register a typed `HttpClient`

Inside your backend project (in`Program.cs` or the dependency injection configuration):

```csharp
builder.Services.AddHttpClient<IPimService, PimService>(client =>
{
    client.BaseAddress = new Uri("http://productCatalog"); // Aspire sets up this URL for local dev
});
```

> **Note:** When running locally with .NET Aspire, `productCatalog` is automatically registered as a known service name and mapped through Aspire’s local service discovery.

---

### Step 2 – Implement the `PimService` class

Create a new file:
`/Backend/Services/PimService.cs`

Here’s an **outline** to guide you:

```csharp
using System.Net.Http.Json;

public class PimService(HttpClient httpClient) : IPimService
{
    public async Task<ICollection<Product>> GetAllProductsAsync(string? authHeader = null)
    {
        //Implementation
    }

    public async Task<Product> GetProductByIdAsync(Guid id, string? authHeader = null)
    {
        //Implementation
    }

    public async Task<(bool Success, string Data, int StatusCode)> CreateProductAsync(Product product, string? authHeader = null)
    {
        //Implementation
    }

    public async Task<(bool Success, string ErrorMessage, int StatusCode)> DeleteProductAsync(Guid id, string? authHeader = null)
    {
        //Implementation
    }

}
```


---

##  Part 4 – Expose the PIM Integration via REST

Now that your **`PimService`** implementation is complete, you’ll make its functionality accessible through an HTTP API in your Backend project.
This will allow frontend or external callers to query and manipulate product data through your backend layer.

You may choose between **Minimal API endpoints** or a **classic MVC Controller** approach.
Either one is acceptable — choose the style you’re most comfortable with.

---

### 4.1  Option A – Minimal API Approach

Open (or create) a configuration file, typically `Program.cs` or `Endpoints.cs`, and register your routes:

```csharp
var group = app.MapGroup("/api/products");

group.MapGet("/", async (IPimService pimService) =>
{
    var products = await pimService.GetAllProductsAsync();
    return Results.Ok(products);
});

group.MapGet("/{id:guid}", async (Guid id, IPimService pimService) =>
{
    var product = await pimService.GetProductByIdAsync(id);
    return Results.Ok(product);
});

group.MapPost("/", async (Product product, IPimService pimService) =>
{
    var result = await pimService.CreateProductAsync(product);
    return Results.StatusCode(result.StatusCode);
});

group.MapDelete("/{id:guid}", async (Guid id, IPimService pimService) =>
{
    var result = await pimService.DeleteProductAsync(id);
    return result.Success
        ? Results.NoContent()
        : Results.StatusCode(result.StatusCode);
});
```

> **Note:** These endpoints proxy calls through your `IPimService`, which in turn communicates with the PIM system running in Aspire (`http://productCatalog`).

When you start the solution, you can now access the api from the URL in the dashboard for backend:

---

### 4.2  Option B – Controller Approach

If you prefer to use classical controllers, create a new file under your Backend project:
`Controllers/ProductsController.cs`

```csharp
[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    private readonly IPimService _pimService;

    public ProductsController(IPimService pimService)
    {
        _pimService = pimService;
    }

    [HttpGet]
    public async Task<IActionResult> GetProducts()
        => Ok(await _pimService.GetAllProductsAsync());

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetProduct(Guid id)
        => Ok(await _pimService.GetProductByIdAsync(id));

    [HttpPost]
    public async Task<IActionResult> CreateProduct([FromBody] Product product)
    {
        var result = await _pimService.CreateProductAsync(product);
        return StatusCode(result.StatusCode, result.Data);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteProduct(Guid id)
    {
        var result = await _pimService.DeleteProductAsync(id);
        return StatusCode(result.StatusCode, result.ErrorMessage);
    }
}
```
---

## 4. Verification

Once implemented:

1. **Run the solution** via Aspire or Visual Studio.
2. Use the backend API explorer call your `PimService`.
3. Confirm that:
   - `GetAllProductsAsync()` returns a valid list.
   - `GetProductByIdAsync()` retrieves a product by ID.
   - Create/Delete operations return expected status codes.

You can verify responses through the **Aspire dashboard logs** or your API test tool (e.g., Postman, curl).

---

## 5. Discussion

This exercise demonstrated:
- Using `HttpClient` effectively in .NET (typed clients)
- Integrating with external systems through dependency injection
- Working with Aspire’s service discovery and orchestration
- Writing clean integration code that can later be reused in Azure-hosted environments

---

## 6. Troubleshooting

| Issue | Cause | Fix |
|-------|-------|-----|
| 404 response | Incorrect endpoint | Check your base URL or route prefix (`/api/products`) |
| Connection refused | PIM service not started | Verify PIM is running in Aspire dashboard |
| Empty product list | Database not seeded | Use CRUD API on the PIM project to add products |

---

## 7. Next Steps

✅ You’ve completed the PIM integration!

**Next:** [Exercise 02-2 – Implementing Payment Service »](./2_2_ImplementingPaymentService.md)
