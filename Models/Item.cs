﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace WarehouseManager.Models
{
    public class Item
    {
        [Key]
        public int ItemID { get; set; }
        [Required(ErrorMessage = "Please enter item name")]
        [DisplayName("Name")]
        public String ItemName { get; set; }
        [DisplayName("Description")]
        public String ItemDescription  { get; set; }
        [Required(ErrorMessage ="Please enter item price")]
        [DisplayFormat(DataFormatString= "{0:C2}")]
        public Double Price { get; set; }
        [Required(ErrorMessage = "Please enter amount of your item")]
        [Range(0, Double.PositiveInfinity, ErrorMessage = "Number of Item must be more than 0")]
        [DisplayName("Number of Item")]
        public int ItemAmount { get; set; }

        public Item() { }
    }
}
