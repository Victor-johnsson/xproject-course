using Microsoft.AspNetCore.Mvc;
using XProjectIntegrationsBackend.Models;
using XProjectIntegrationsBackend.Services;

namespace XProjectIntegrationsBackend.Controllers;

[Route("/payments")]
[ApiController]
public class PaymentsController : ControllerBase
{
    private readonly IPaymentsService _paymentsService;

    public PaymentsController(IPaymentsService paymentsService)
    {
        _paymentsService = paymentsService;
    }

    [HttpPost]
    public async Task<IResult> CreatePayments()
    {
        return await _paymentsService.CreatePaymentsAsync();
    }

    [HttpPut]
    public async Task<IResult> UpdatePayments(Payment payment)
    {
        return await _paymentsService.UpdatePaymentsAsync(payment);
    }

    [HttpGet]
    public async Task<IResult> GetPayments()
    {
        return await _paymentsService.GetPaymentsAsync();
    }

    [HttpGet("{id}")]
    public async Task<IResult> GetPaymentsById(int id)
    {
        return await _paymentsService.GetPaymentsByIdAsync(id);
    }

    [HttpDelete("{id}")]
    public async Task<IResult> DeletePaymentsById(int id)
    {
        return await _paymentsService.DeletePaymentsByIdAsync(id);
    }
}
