using Microsoft.EntityFrameworkCore;
using XProject.Integrations.CRM.EntityFramework;
using XProject.Integrations.CRM.EntityFramework.Entities;
using XProject.Integrations.CRM.Enums;
using XProject.Integrations.CRM.Models.RequestModels;

namespace XProject.Integrations.CRM.Services
{

    public interface IOrderService
    {
        Task<OrderDTO> CreateOrder(CreateOrderModel model);
        //Task GetOrders();
        //Task GetOrder();
        Task<List<OrderDTO>> UpdateOrderStatus();

    }
    public class OrderService : IOrderService
    {
        CrmContext _context;

        public OrderService(CrmContext context)
        {
            _context = context;
        }

        public async Task<OrderDTO> CreateOrder(CreateOrderModel model)
        {
            Customer customer = new Customer()
            {
                Name = model.Customer?.Name,
                Address = model.Customer?.Address,
                Email = model.Customer?.Email,
                Order = new Order()
                {
                    PaymentId = model.PaymentId,
                    Status = OrderStatusEnum.OrderReceived.ToString(),
                    OrderLines = model.OrderLines?.Select(p => new OrderLine
                    {
                        ProdRef = p.ProductId,
                        ItemCount = p.ItemCount

                    }).ToList()
                }
            };

            _context.Customers.Add(customer);
            var v = await _context.SaveChangesAsync();
            if (v >1){
                OrderDTO orderDTO = new OrderDTO
                {
                    OrderId = customer.Order.Id,
                    CustomerName = customer.Name,
                    CustomerAddress = customer.Address,
                    CustomerEmail = customer.Email,
                    OrderStatus = OrderStatusEnum.OrderReceived.ToString(),
                };
                return orderDTO;
            }

            throw new Exception();
        }

        //public async Task GetOrders()
        //{

        //}

        public async Task<List<OrderDTO>> UpdateOrderStatus()
        {
            var orders = await _context.Orders.Where(x => x.Status == OrderStatusEnum.OrderReceived.ToString()).ToListAsync();
            foreach (var order in orders)
            {
                order.Status = OrderStatusEnum.OrderReadyForPickUp.ToString();
            }

            await _context.SaveChangesAsync();

            var allOrders = await _context.Orders.Include(o => o.Customer).Include(o => o.OrderLines).Where(x => x.Status == OrderStatusEnum.OrderReadyForPickUp.ToString()).ToListAsync();
            var dtoList = new List<OrderDTO>();
            foreach (var order in allOrders)
            {
                var dtoObj = new OrderDTO
                {
                    OrderId = order.Id,
                    CustomerName = order.Customer?.Name,
                    OrderStatus = order.Status,
                    CustomerEmail = order.Customer?.Email,
                    CustomerAddress = order.Customer?.Address
                };
                dtoList.Add(dtoObj);
            }

            return dtoList;
        }
    }

    public class OrderDTO
    {
        public int OrderId { get; set; }
        public string? CustomerName { get; set; }
        public string? OrderStatus { get; set; }
        public string? CustomerEmail { get; set; }
        public string? CustomerAddress { get; set; }
    }
}

