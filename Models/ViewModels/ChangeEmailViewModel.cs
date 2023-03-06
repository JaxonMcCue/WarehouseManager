using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace WarehouseManager.Models.ViewModels
{
    public class ChangeEmailViewModel
    {
        public string Username { get; set; } = string.Empty;

        [Required(ErrorMessage = "Please enter a new email")]
        [RegularExpression(@"^[a-z0-9]+@[a-z]+\.[a-z]{2,3}$",
            ErrorMessage = "Invalid Email")]
        public string NewEmail { get; set; } = string.Empty;

    }
}
