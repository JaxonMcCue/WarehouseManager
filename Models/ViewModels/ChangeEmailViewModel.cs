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

        [Required(ErrorMessage = "Please enter your new email address.")]
        [EmailAddress(ErrorMessage = "Please enter a valid email address.")]
        [Display(Name = "New Email")]
        public string NewEmail { get; set; }

        [Required(ErrorMessage = "Please confirm your new email address.")]
        [Compare("NewEmail", ErrorMessage = "The email and confirmation email do not match.")]
        [Display(Name = "Confirm New Email")]
        public string ConfirmEmail { get; set; }
        [Required(ErrorMessage ="Please enter your password")]
        public string ConfirmPassword { get; set; }
    }
}
