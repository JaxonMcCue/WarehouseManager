using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WarehouseManager.Models
{
    public class Role : IdentityRole
    {
        public override string Name { get; set; }

        public Role() { }

        public Role(string name)
        {
            Name = name;
        }
    }
}
