using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WarehouseManager.Models
{
    public class Refund
    {
        public int RefundID { get; set; }
        public int OrderID { get; set; }
        public Order Order { get; set; }
        public string Reason { get; set; }
        public bool? Confirmed { get; set; }
    }
}
