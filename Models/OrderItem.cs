using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WarehouseManager.Models
{
    public class OrderItem
    {
        public int ID { get; set; }
        public int ItemID { get; set; }
        public Item Item { get; set; }
        public int OrderID { get; set; }
    }
}
