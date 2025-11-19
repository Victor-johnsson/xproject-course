using System.Text;
using System.Text.Json;
using Azure.Data.Tables;
using Azure.Messaging.ServiceBus;
using Azure.Security.KeyVault.Secrets;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Moq;
using WebShopX.FunctionService.Api.Functions;
using WebShopX.FunctionService.Core.Models;
using WebShopX.FunctionService.Services;

namespace WebShopX.FunctionApp.Tests
{
    public class ServiceBusTransportFunctionTests
    {
        private readonly Mock<TableServiceClient> _mockTableServiceClient;
        private readonly Mock<ILogger<TransportFunction>> _mockLogger;
        private readonly Mock<ITableStorageService> _mockTableStorageService;
        private readonly TransportFunction _function;

        public ServiceBusTransportFunctionTests()
        {
            _mockLogger = new Mock<ILogger<TransportFunction>>();
            _mockTableServiceClient = new Mock<TableServiceClient>();
            _mockTableStorageService = new Mock<ITableStorageService>();
            _function = new TransportFunction(
                _mockTableServiceClient.Object,
                _mockLogger.Object,
                _mockTableStorageService.Object
            );
        }

        [Test]
        public async Task ServiceBusTransportFunction_Should_Process_Valid_Message()
        {
            // Arrange
            var orderMessage = new Order
            {
                Status = "Pending",
                PaymentId = "3",
                Customer = new Customer
                {
                    Name = "John Doe",
                    Address = "123 Elm Street, NY, USA",
                    Email = "johndoe@example.com",
                },
                OrderLines = new List<OrderLine>
                {
                    new OrderLine
                    {
                        ProductId = "01959381-773b-78d6-b50f-f3d0be4e2523",
                        ItemCount = 2,
                    },
                    new OrderLine
                    {
                        ProductId = "01958af3-4a85-7d99-9b31-71b702a18ca5",
                        ItemCount = 1,
                    },
                },
            };

            var messageBody = JsonSerializer.Serialize(orderMessage);
            var message = CreateServiceBusMessage(messageBody);
            var mockMessageActions = new Mock<ServiceBusMessageActions>();

            // Act
            await _function.ServiceBusTransportFunction(message, mockMessageActions.Object);

            // Assert: Verify UpdateOrderStatus was called exactly once
            _mockTableStorageService.Verify(
                service => service.UpdateOrderStatus(It.IsAny<TransportationEntity>()),
                Times.Once()
            );

            // Assert: Verify logging occurred with the expected information
            _mockLogger.Verify(
                log =>
                    log.Log(
                        It.Is<LogLevel>(l => l == LogLevel.Information),
                        It.IsAny<EventId>(),
                        It.Is<It.IsAnyType>(
                            (obj, type) => obj.ToString().Contains("Received message")
                        ),
                        null,
                        (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()
                    ),
                Times.Once
            );

            Dispose();
        }

        [Test]
        public async Task ServiceBusTransportFunction_Should_LogError_On_Deserialization_Failure()
        {
            // Arrange
            var invalidJsonMessage = "{ invalid json }";
            var message = CreateServiceBusMessage(invalidJsonMessage);
            var mockMessageActions = new Mock<ServiceBusMessageActions>();

            // Act
            await _function.ServiceBusTransportFunction(message, mockMessageActions.Object);

            // Assert: Verify that the deserialization error was logged
            _mockLogger.Verify(
                x =>
                    x.Log(
                        LogLevel.Error,
                        It.IsAny<EventId>(),
                        It.Is<It.IsAnyType>(
                            (o, t) => o.ToString().Contains("Error deserializing message")
                        ),
                        It.IsAny<Exception>(),
                        It.IsAny<Func<It.IsAnyType, Exception, string>>()
                    ),
                Times.Once
            );

            // Assert: Verify that UpdateOrderStatus was NOT called (since deserialization failed)
            _mockTableStorageService.Verify(
                service => service.UpdateOrderStatus(It.IsAny<TransportationEntity>()),
                Times.Never
            );

            Dispose();
        }

        [Test]
        public async Task ServiceBusTransportFunction_Should_LogError_If_Required_Field_Is_Missing()
        {
            // Arrange:
            var orderMessage = new Order
            {
                Status = "Pending",
                PaymentId = string.Empty,
                Customer = new Customer
                {
                    Name = "John Doe",
                    Address = "123 Elm Street, NY, USA",
                    Email = "johndoe@example.com",
                },
                OrderLines = new List<OrderLine>
                {
                    new OrderLine
                    {
                        ProductId = "01959381-773b-78d6-b50f-f3d0be4e2523",
                        ItemCount = 2,
                    },
                    new OrderLine
                    {
                        ProductId = "01958af3-4a85-7d99-9b31-71b702a18ca5",
                        ItemCount = 1,
                    },
                },
            };

            var messageBody = JsonSerializer.Serialize(orderMessage);
            var message = CreateServiceBusMessage(messageBody);
            var mockMessageActions = new Mock<ServiceBusMessageActions>();

            await _function.ServiceBusTransportFunction(message, mockMessageActions.Object);

            // Assert: Verify that UpdateOrderStatus method was NOT called, as Status is missing
            _mockTableStorageService.Verify(
                service => service.UpdateOrderStatus(It.IsAny<TransportationEntity>()),
                Times.Never
            );

            // Assert: Verify that an error log was recorded due to the missing Status field
            _mockLogger.Verify(
                logger =>
                    logger.Log(
                        It.Is<LogLevel>(level => level == LogLevel.Error),
                        It.IsAny<EventId>(),
                        It.IsAny<It.IsAnyType>(),
                        It.IsAny<Exception>(),
                        It.IsAny<Func<It.IsAnyType, Exception, string>>()
                    ),
                Times.Once
            ); // Expect the error log to be called exactly once

            Dispose();
        }

        [Test]
        public async Task ServiceBusTransportFunction_Should_Handle_UpdateOrderStatus_Exception()
        {
            // Arrange
            var orderMessage = new Order
            {
                Status = "Pending",
                PaymentId = "3",
                Customer = new Customer
                {
                    Name = "John Doe",
                    Address = "123 Elm Street, NY, USA",
                    Email = "johndoe@example.com",
                },
                OrderLines = new List<OrderLine>
                {
                    new OrderLine
                    {
                        ProductId = "01959381-773b-78d6-b50f-f3d0be4e2523",
                        ItemCount = 2,
                    },
                    new OrderLine
                    {
                        ProductId = "01958af3-4a85-7d99-9b31-71b702a18ca5",
                        ItemCount = 1,
                    },
                },
            };

            var messageBody = JsonSerializer.Serialize(orderMessage);
            var message = CreateServiceBusMessage(messageBody);
            var mockMessageActions = new Mock<ServiceBusMessageActions>();

            // Simulate an exception in UpdateOrderStatus()
            _mockTableStorageService
                .Setup(service => service.UpdateOrderStatus(It.IsAny<TransportationEntity>()))
                .ThrowsAsync(new System.Exception("Database connection failure"));

            // Act
            await _function.ServiceBusTransportFunction(message, mockMessageActions.Object);

            // Assert: Verify Log was called with the expected error message when UpdateOrderStatus fails
            _mockLogger.Verify(
                logger =>
                    logger.Log(
                        It.Is<LogLevel>(level => level == LogLevel.Error),
                        It.IsAny<EventId>(),
                        It.Is<It.IsAnyType>(
                            (o, t) =>
                                o.ToString()
                                    .Contains(
                                        "Error updating order status: Database connection failure"
                                    )
                        ), // Ensure the error message is correct
                        It.IsAny<Exception>(),
                        It.IsAny<Func<It.IsAnyType, Exception, string>>()
                    ),
                Times.Once
            );

            Dispose();
        }

        [Test]
        public async Task ServiceBusTransportFunction_Should_Update_Order_Status_In_Table()
        {
            // Arrange
            var orderMessage = new Order
            {
                Status = "Shipped",
                PaymentId = "12345",
                Customer = new Customer
                {
                    Name = "John Doe",
                    Address = "123 Main St",
                    Email = "john.doe@example.com",
                },
                OrderLines = new List<OrderLine>
                {
                    new OrderLine
                    {
                        ProductId = "01959381-773b-78d6-b50f-f3d0be4e2523",
                        ItemCount = 2,
                    },
                    new OrderLine
                    {
                        ProductId = "01958af3-4a85-7d99-9b31-71b702a18ca5",
                        ItemCount = 1,
                    },
                },
            };

            var messageBody = JsonSerializer.Serialize(orderMessage);
            var message = CreateServiceBusMessage(messageBody);
            var mockMessageActions = new Mock<ServiceBusMessageActions>();

            _mockTableStorageService
                .Setup(service => service.UpdateOrderStatus(It.IsAny<TransportationEntity>()))
                .Callback<TransportationEntity>(
                    (entity) =>
                    {
                        // Assert that correct values are being passed to UpdateOrderStatus
                        Assert.Equals("DHLTest4", entity.PartitionKey);
                        Assert.Equals("12345", entity.RowKey);
                        Assert.Equals("Shipped", entity.Status);
                        Assert.Equals("John Doe", entity.CustomerName);
                        Assert.Equals("john.doe@example.com", entity.CustomerEmail);
                        Assert.Equals("123 Main St", entity.CustomerAddress);
                    }
                )
                .Returns(Task.CompletedTask);

            // Act
            await _function.ServiceBusTransportFunction(message, mockMessageActions.Object);

            // Assert: Verify that UpdateOrderStatus was called exactly once with the correct entity
            _mockTableStorageService.Verify(
                service => service.UpdateOrderStatus(It.IsAny<TransportationEntity>()),
                Times.Once
            );

            // Verify that an informational log entry was made about the received message
            _mockLogger.Verify(
                log =>
                    log.Log(
                        It.Is<LogLevel>(l => l == LogLevel.Information),
                        It.IsAny<EventId>(),
                        It.Is<It.IsAnyType>(
                            (obj, type) => obj.ToString().Contains("Received message")
                        ),
                        null,
                        It.IsAny<Func<It.IsAnyType, Exception, string>>()
                    ),
                Times.Once
            );

            // Verify that an error log was made if there were any issues in the function
            _mockLogger.Verify(
                log =>
                    log.Log(
                        It.Is<LogLevel>(l => l == LogLevel.Error),
                        It.IsAny<EventId>(),
                        It.Is<It.IsAnyType>(
                            (obj, type) => obj.ToString().Contains("Error updating order status")
                        ),
                        It.IsAny<Exception>(),
                        It.IsAny<Func<It.IsAnyType, Exception, string>>()
                    ),
                Times.Never
            ); // Ensure no errors were logged in this successful scenario

            Dispose();
        }

        [Test]
        public async Task ServiceBusTransportFunction_Should_Write_New_Entity_To_Table()
        {
            // Arrange
            var orderMessage = new Order
            {
                Status = "Shipped",
                PaymentId = "12345",
                Customer = new Customer
                {
                    Name = "John Doe",
                    Address = "123 Main St",
                    Email = "john.doe@example.com",
                },
                OrderLines = new List<OrderLine>
                {
                    new OrderLine
                    {
                        ProductId = "01959381-773b-78d6-b50f-f3d0be4e2523",
                        ItemCount = 2,
                    },
                    new OrderLine
                    {
                        ProductId = "01958af3-4a85-7d99-9b31-71b702a18ca5",
                        ItemCount = 1,
                    },
                },
            };

            var messageBody = JsonSerializer.Serialize(orderMessage);
            var message = CreateServiceBusMessage(messageBody);
            var mockMessageActions = new Mock<ServiceBusMessageActions>();

            // Setup the mock for UpdateOrderStatus to capture the entity being passed
            _mockTableStorageService
                .Setup(service => service.UpdateOrderStatus(It.IsAny<TransportationEntity>()))
                .Callback<TransportationEntity>(
                    (entity) =>
                    {
                        Assert.Equals("DHLTest4", entity.PartitionKey);
                        Assert.Equals("12345", entity.RowKey);
                        Assert.Equals("Shipped", entity.Status);
                        Assert.Equals("John Doe", entity.CustomerName);
                        Assert.Equals("john.doe@example.com", entity.CustomerEmail);
                        Assert.Equals("123 Main St", entity.CustomerAddress);
                    }
                )
                .Returns(Task.CompletedTask);

            // Act
            await _function.ServiceBusTransportFunction(message, mockMessageActions.Object);

            // Assert: Verify that UpdateOrderStatus was called exactly once with the correct entity
            _mockTableStorageService.Verify(
                service => service.UpdateOrderStatus(It.IsAny<TransportationEntity>()),
                Times.Once
            );

            // Verify that an informational log entry was made about the received message
            _mockLogger.Verify(
                log =>
                    log.Log(
                        It.Is<LogLevel>(l => l == LogLevel.Information),
                        It.IsAny<EventId>(),
                        It.Is<It.IsAnyType>(
                            (obj, type) => obj.ToString().Contains("Received message")
                        ),
                        null,
                        It.IsAny<Func<It.IsAnyType, Exception, string>>()
                    ),
                Times.Once
            );

            // Verify that an error log was made if there were any issues in the function
            _mockLogger.Verify(
                log =>
                    log.Log(
                        It.Is<LogLevel>(l => l == LogLevel.Error),
                        It.IsAny<EventId>(),
                        It.Is<It.IsAnyType>(
                            (obj, type) => obj.ToString().Contains("Error updating order status")
                        ),
                        It.IsAny<Exception>(),
                        It.IsAny<Func<It.IsAnyType, Exception, string>>()
                    ),
                Times.Never
            ); // Ensure no errors were logged in this successful scenario

            Dispose();
        }

        private ServiceBusReceivedMessage CreateServiceBusMessage(string body)
        {
            var messageData = Encoding.UTF8.GetBytes(body);
            return ServiceBusModelFactory.ServiceBusReceivedMessage(
                body: BinaryData.FromBytes(messageData)
            );
        }

        private void Dispose()
        {
            _mockLogger.Reset();
            _mockTableStorageService.Reset();
        }
    }
}
