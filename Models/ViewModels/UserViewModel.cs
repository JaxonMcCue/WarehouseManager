using System.Collections.Generic;
using WarehouseManager.Models;

namespace WarehouseManager.Models.ViewModels
{
    public class UserViewModel
    {
            public IEnumerable<User> Users { get; set; }
            public IEnumerable<Role> Roles { get; set; }

            public string UserId { get; set; }
            public string RoleName { get; set; }
        
    }
}