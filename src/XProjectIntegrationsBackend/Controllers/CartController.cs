using Microsoft.AspNetCore.Mvc;
using XProjectIntegrationsBackend.Interfaces;
using XProjectIntegrationsBackend.Models;

namespace XProjectIntegrationsBackend.Controllers
{
    [Route("/cart")]
    [ApiController]
    public class CartController : ControllerBase
    {
        private readonly IRedisCacheService _cacheService;

        public CartController(IRedisCacheService cacheService)
        {
            _cacheService = cacheService;
        }

        [HttpPost("add")]
        public async Task<IActionResult> AddToCart(
            [FromQuery] string sessionId,
            [FromBody] CartItem item
        )
        {
            if (string.IsNullOrEmpty(sessionId))
                return BadRequest("Session ID is required.");

            await _cacheService.AddToCartAsync(sessionId, item);
            return Ok(new { message = "Item added to cart." });
        }

        [HttpGet]
        public async Task<IActionResult> GetCart([FromQuery] string sessionId)
        {
            if (string.IsNullOrEmpty(sessionId))
                return BadRequest("Session ID is required.");

            List<CartItem> cart = await _cacheService.GetCartAsync(sessionId);
            return Ok(cart);
        }

        [HttpDelete("remove")]
        public async Task<IActionResult> RemoveFromCart(
            [FromQuery] string sessionId,
            [FromQuery] string productId
        )
        {
            if (string.IsNullOrEmpty(sessionId) || string.IsNullOrEmpty(productId))
                return BadRequest("Session ID and Product ID are required.");

            await _cacheService.RemoveFromCartAsync(sessionId, productId);
            return Ok(new { message = "Item removed from cart." });
        }

        [HttpDelete("clear")]
        public async Task<IActionResult> ClearCart([FromQuery] string sessionId)
        {
            if (string.IsNullOrEmpty(sessionId))
                return BadRequest("Session ID is required.");

            await _cacheService.ClearCartAsync(sessionId);
            return Ok(new { message = "Cart cleared." });
        }
    }
}
