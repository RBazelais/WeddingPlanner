using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WeddingPlanner.Models
{
    public class RegisterViewModel : BaseEntity
    {
        [Required]
        [MinLength(4, ErrorMessage = "First name should be atleast 4 characters long")]
        public string FirstName { get; set; }

        [Required]
        [MinLength(4, ErrorMessage = "Last name should be atleast 4 characters long")]
        public string LastName { get; set; }

        [Required]
        [EmailAddress(ErrorMessage="Please ensure that you have entered a valid email address")]
        public string Email { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [MinLength(8, ErrorMessage = "Password must have at least 8 characters")]
        public string Password { get; set; }

        [Required]
        [Compare ("Password", ErrorMessage = "Passwords much match")]
        [DataType(DataType.Password)]
        [MinLength(8, ErrorMessage = "Password must have at least 8 characters")]
        public string ConfirmPassword { get; set; }
    }
}