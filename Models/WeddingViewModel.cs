using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using WeddingPlanner.Models.CustomValidations;

namespace WeddingPlanner.Models
{
    public class WeddingViewModel : BaseEntity
    {
        [Required]
        [MinLength(3, ErrorMessage = "First partner's name should be atleast 3 characters long")]
        public string P1Name { get; set; }

        [Required]
        [MinLength(3, ErrorMessage = "Second partner's name should be atleast 3 characters long")]
        public string P2Name { get; set; }

        [Required]
        [InTheFuture(ErrorMessage = "Date must be set in the future")]
        [DataType(DataType.Date)]
        public DateTime Date { get; set; }

        [Required]
        public string Address { get; set; }

    }
}