using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace WarehouseManager.Models
{
    public class Order
    {
        public int OrderID { get; set; }
        public int CustomerID { get; set; }
        [DataType(DataType.Currency)]
        [Column(TypeName = "decimal(18,2)")]
        public decimal OrderCost { get; set; }
        public int ItemCount { get; set; }
        public bool Completed { get; set; }
        public bool Cancelled { get; set; }

        public Order() { }

        public Order(decimal orderCost)
        {
            OrderCost = orderCost;
            ItemCount = 0;
            Completed = false;
            Cancelled = false;
        }
    }
}
