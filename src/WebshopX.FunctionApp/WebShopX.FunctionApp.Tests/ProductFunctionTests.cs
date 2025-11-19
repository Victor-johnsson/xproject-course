using System.Text.Json;
using Moq;
using Microsoft.Extensions.Logging;
using WebShopX.FunctionService.Services;
using Azure.Messaging.EventGrid;
using WebShopX.FunctionService.Api.Functions;
using WebShopX.FunctionService.Core.Models;
using Azure.Messaging;
namespace WebShopX.FunctionApp.Tests
{
    public class ProductFunctionTests
    {
        private Mock<ILogger<ProductFunction>> _mockLogger;
        private Mock<IPimService> _mockPimService;

        [SetUp]
        public void Setup()
        {
            _mockLogger = new Mock<ILogger<ProductFunction>>();
            _mockPimService = new Mock<IPimService>();
        }

        [Test]
        public async Task UpdateProductCount_Should_Invoke_UpdateStock_With_Correct_Parameters()
        {
            // Arrange
            var eventData = new Order
            {
                Status = "Pending",
                PaymentId = "1",
                Customer = new Customer
                {
                    Name = "John Doe",
                    Address = "123 Main St",
                    Email = "john@example.com"
                },
                OrderLines = new List<OrderLine>
                {
                    new OrderLine
                    {
                        ItemCount = 2,
                        ProductId = "01958ae82"
                    },
                    new OrderLine
                    {
                        ItemCount = 5,
                        ProductId = "019593ba2"
                    }
                }
            };

            string jsonData = JsonSerializer.Serialize(eventData);

            var testEvent = new CloudEvent(
                 source: "products/123",  
                 type: "Product.StockUpdated",
                 jsonSerializableData: jsonData  
             )
            {
                Id = Guid.NewGuid().ToString(),
                Time = DateTimeOffset.UtcNow  
            };

            var mockLogger = new Mock<ILogger<ProductFunction>>();
            var mockPimService = new Mock<IPimService>();

            mockPimService
                .Setup(service => service.UpdateStock(It.IsAny<string>(), It.IsAny<int>()));

            var productFunction = new ProductFunction(mockLogger.Object, mockPimService.Object);

            // Act
            await productFunction.UpdateProductCount(testEvent);

            // Assert
            mockPimService.Verify(service => service.UpdateStock("01958ae82", 2), Times.Once);
            mockPimService.Verify(service => service.UpdateStock("019593ba2", 5), Times.Once);

            // Ensure no unexpected calls happened
            mockPimService.VerifyNoOtherCalls();
        }

    }
}
