using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WarehouseManager.Models
{
    public class Order
    {
        public int OrderID { get; set; }
        public int CustomerID { get; set; }
        public decimal OrderCost { get; set; }
        public bool Completed { get; set; }
        public bool Cancelled { get; set; }

        public Order() { }

        public Order(decimal orderCost)
        {
            OrderCost = orderCost;
            Completed = false;
            Cancelled = false;
        }
    }
}
