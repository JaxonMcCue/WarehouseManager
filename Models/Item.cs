using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WarehouseManager.Models
{
    public class Item
    {
        public int ItemID { get; set; }
        public String ItemName { get; set; }
        public String ItemDescription  { get; set; }
        public Double Price { get; set; }
        public int ItemAmount { get; set; }
    }
}
