using Microsoft.AspNetCore.Mvc;
using XProject.Integrations.CRM.Authentication;
using XProject.Integrations.CRM.Models.RequestModels;
using XProject.Integrations.CRM.Services;

namespace XProject.Integrations.CRM.Controllers
{
    [ApiController]
    public class CrmController : ControllerBase
    {
        private readonly ILogger<CrmController> _logger;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IOrderService _orderService;


        public CrmController(ILogger<CrmController> logger, IHttpContextAccessor httpContextAccessor, IOrderService orderService)
        {
            _logger = logger;
            _httpContextAccessor = httpContextAccessor;
            _orderService = orderService;
        }

        [HttpGet("crm/api/healthCheck")]
        public async Task<IActionResult> HealthCheck()
        {

            return Ok();
        }

        [HttpPost("crm/api/order")]
        public async Task<IActionResult> CreateOrder([FromBody] CreateOrderModel model)
        {
            //if (!AuthValidator.Validate(_httpContextAccessor))
            //{
            //    return Unauthorized();
            //}

            // Create new order
            var dto = await _orderService.CreateOrder(model);
            return Ok(dto);

        }

        //[HttpGet("crm/api/order/{orderId}")]
        //public async Task<IActionResult> GetOrder(string orderId)
        //{
        //    if (!AuthValidator.Validate(_httpContextAccessor))
        //    {
        //        return Unauthorized();
        //    }

        //    if (string.IsNullOrWhiteSpace(orderId))
        //    {
        //        return BadRequest("Missing orderId");
        //    }

        //    return Ok();
        //}

        //[HttpGet("crm/api/orders")]
        //public async Task<IActionResult> GetOrders(string orderId)
        //{
        //    if (!AuthValidator.Validate(_httpContextAccessor))
        //    {
        //        return Unauthorized();
        //    }

        //    if (string.IsNullOrWhiteSpace(orderId))
        //    {
        //        return BadRequest("Missing orderId");
        //    }

        //    return Ok();
        //}

        [HttpPatch("crm/api/order")]
        public async Task<IActionResult> UpdateOrderStatus()
        {
            //if (!AuthValidator.Validate(_httpContextAccessor))
            //{
            //    return Unauthorized();
            //}

            return Ok(await _orderService.UpdateOrderStatus());
        }
    }
}
