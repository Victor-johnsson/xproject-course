using System.Text;
using Microsoft.AspNetCore.Mvc;
using XProjectIntegrationsBackend.Models;

namespace XProjectIntegrationsBackend.Services;

public interface IPaymentsService
{
    Task<IResult> CreatePaymentsAsync();
    Task<IResult> UpdatePaymentsAsync(Payment payment);
    Task<IResult> GetPaymentsAsync();
    Task<IResult> GetPaymentsByIdAsync(int id);
    Task<IResult> DeletePaymentsByIdAsync(int id);
}

public class PaymentsService : IPaymentsService
{
    private readonly ILogger<PaymentsService> _logger;
    private readonly HttpClient _httpClient;

    public PaymentsService(HttpClient httpClient, ILogger<PaymentsService> logger)
    {
        _logger = logger;
        _httpClient = httpClient;
    }

    public async Task<IResult> CreatePaymentsAsync()
    {
        try
        {
            using var request = new HttpRequestMessage(HttpMethod.Post, "/payments");
            // request.Headers.Add("Ocp-Apim-Subscription-Key", apimKey);
            var response = await _httpClient.SendAsync(request);

            if (response.IsSuccessStatusCode)
            {
                var paymentId = await response.Content.ReadAsStringAsync();
                return Results.Ok(paymentId);
            }

            return Results.BadRequest("Failed to create payment");
        }
        catch (Exception ex)
        {
            return Results.InternalServerError($"Internal Server Error: {ex.Message}");
        }
    }

    public async Task<IResult> DeletePaymentsByIdAsync(int id)
    {
        try
        {
            var request = new HttpRequestMessage(HttpMethod.Delete, $"/payments/{id}");
            // request.Headers.Add("Ocp-Apim-Subscription-Key", apimKey);

            var response = await _httpClient.SendAsync(request);
            if (!response.IsSuccessStatusCode)
            {
                return TypedResults.NotFound("Payment not found.");
            }

            var data = await response.Content.ReadAsStringAsync();
            return TypedResults.Content(data, "application/json");
        }
        catch (Exception ex)
        {
            return TypedResults.Problem(
                new()
                {
                    Status = 500,
                    Detail = $"Internal Server Error: {ex.Message}",
                    Title = "Internal Server Error",
                }
            );
        }
    }

    public async Task<IResult> GetPaymentsAsync()
    {
        try
        {
            var request = new HttpRequestMessage(HttpMethod.Get, $"/payments");
            // request.Headers.Add("Ocp-Apim-Subscription-Key", apimKey);

            var response = await _httpClient.SendAsync(request);
            if (!response.IsSuccessStatusCode)
            {
                return TypedResults.NotFound("No payments found");
            }

            var data = await response.Content.ReadAsStringAsync();
            return TypedResults.Content(data, "application/json");
        }
        catch (Exception ex)
        {
            return TypedResults.Problem(
                new()
                {
                    Status = 500,
                    Detail = $"Internal Server Error: {ex.Message}",
                    Title = "Internal Server Error",
                }
            );
        }
    }

    public async Task<IResult> GetPaymentsByIdAsync(int id)
    {
        try
        {
            var request = new HttpRequestMessage(HttpMethod.Get, $"/payments/{id}");
            // request.Headers.Add("Ocp-Apim-Subscription-Key", apimKey);

            var response = await _httpClient.SendAsync(request);
            if (!response.IsSuccessStatusCode)
            {
                return TypedResults.NotFound("Payment not found.");
            }

            var data = await response.Content.ReadAsStringAsync();
            return TypedResults.Content(data, "application/json");
        }
        catch (Exception ex)
        {
            return TypedResults.Problem(
                new()
                {
                    Status = 500,
                    Title = "Internal Server Error",
                    Detail = $"Internal Server Error: {ex.Message}",
                }
            );
        }
    }

    public async Task<IResult> UpdatePaymentsAsync(Payment payment)
    {
        try
        {
            var jsonData = System.Text.Json.JsonSerializer.Serialize(payment);
            using var content = new StringContent(jsonData, Encoding.UTF8, "application/json");

            using var request = new HttpRequestMessage(HttpMethod.Put, "/payments")
            {
                Content = content,
            };
            // request.Headers.Add("Ocp-Apim-Subscription-Key", apimKey);
            var response = await _httpClient.SendAsync(request);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError($"Failed to modify payment. Status: {response.StatusCode}");
                return TypedResults.Problem(
                    new ProblemDetails
                    {
                        Status = (int)response.StatusCode,
                        Title = "Payment modification failed.",
                        Detail = await response.Content.ReadAsStringAsync(),
                    }
                );
            }

            return TypedResults.Ok();
        }
        catch (Exception ex)
        {
            return TypedResults.Problem(
                new()
                {
                    Status = 500,
                    Title = "Internal Server Error",
                    Detail = $"Internal Server Error: {ex.Message}",
                }
            );
        }
    }
}
