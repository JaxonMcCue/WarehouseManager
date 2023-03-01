using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace WarehouseManager.Models
{
    public class User : IdentityUser
    {
        public int CustomerID { get; set; }

        //public Customer customer { get; set; }

        [NotMapped]
        public IList<String> RoleNames { get; set; }

        public User() { }

    }
}
