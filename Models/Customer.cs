using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace WarehouseManager.Models
{
    public class Customer
    {
        public int CustomerID { get; set; }
        [Required(ErrorMessage = "First Name is Required")]
        [DisplayName("Business Name")]
        public string BusinessName { get; set; }
        [DisplayName("First Name")]
        public string FirstName { get; set; }
        [Required(ErrorMessage = "Last Name is Required")]
        [DisplayName("Last Name")]
        public string LastName { get; set; }
        [Required(ErrorMessage = "Address is Required")]
        public string Address { get; set; }
        [Required(ErrorMessage = "City is required")]
        public string City { get; set; }
        [Required(ErrorMessage = "State is required")]
        [RegularExpression(@"(A[KLRZ]|C[AOT]|D[CE]|FL|GA|HI|I[ADLN]|K[SY]|LA|M[ADEINOST]|N[CDEHJMVY]|O[HKR]|PA|RI|S[CD]|T[NX]|UT|V[AT]|W[AIVY])",
            ErrorMessage = "Invalid State")]
        public string State { get; set; }
        [Required(ErrorMessage = "Zip is required")]
        [RegularExpression(@"(^\d{5}$)|(^\d{9}$)| (^\d{ 5}-\d{ 4}$)", ErrorMessage = "Invalid Zipcode")]
        public string Zip { get; set; }
        [Phone]
        [Required(ErrorMessage = "Phone number is Required")]
        [RegularExpression(@"^((\+1|1)?\(?(800|[0-9]{3})(\)?\s?|\s?))?\-?[0-9]{3}(\s|\-)?[0-9]{4}$",
            ErrorMessage = "Invalid Phone Number")]
        public string Phone { get; set; }

        public Customer() { }

        public Customer(int id, string firstName, string lastName, string address, string city, string state, string zip, string phone)
        {
            CustomerID = id;
            FirstName = firstName;
            LastName = lastName;
            Address = address;
            City = city;
            State = state;
            Zip = zip;
            Phone = phone;
        }

    }
}
