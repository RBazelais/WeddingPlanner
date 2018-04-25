using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WeddingPlanner.Models
{
    public class LoginViewModel : BaseEntity
    {

        [Required]
        [EmailAddress(ErrorMessage="Please ensure that you have entered a valid email address")]
        public string Email { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [MinLength(8, ErrorMessage = "Password must have at least 8 characters")]
        public string Password { get; set; }

    }
}