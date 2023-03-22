using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace WarehouseManager.Models
{
    public class DamagedItem
    {
        [Key]
        public int DamagedID { get; set; }
        public int ItemID { get; set; }
        public Item DamagedItems { get; set; }
        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Please enter a value bigger that 0")]
        public int Count { get; set; }
        public string Desc { get; set; }
        public DateTime Date { get; set; }
    }
}
