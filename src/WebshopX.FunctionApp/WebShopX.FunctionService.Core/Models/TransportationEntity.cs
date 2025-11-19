using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebShopX.FunctionService.Core.Models
{
    public class TransportationEntity
    {
        public required string PartitionKey { get; set; }
        public required string RowKey { get; set; }
        public required string Status { get; set; }             
        public required string CustomerName { get; set; }
        public required string CustomerEmail { get; set; }
        public required string CustomerAddress { get; set; }
    }
}
