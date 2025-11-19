using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebShopX.FunctionService.Core.Models
{
    public class ProductStockEvent
    {
        public required string ProductId { get; set; }
        public required int Stock { get; set; }
    }
}
