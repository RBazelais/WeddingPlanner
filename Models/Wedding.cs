using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WeddingPlanner.Models
{
    public class Wedding : BaseEntity
    {
        [Key]
        public int WeddingId { get; set; }

        [Required]
        public string P1Name { get; set; }

        [Required]
        public string P2Name { get; set; }
        
        // [DatabaseGenerated(DatabaseGeneratedOption.Identity)]        
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:MMM-dd-yyyy}")]
        public DateTime? Date { get; set; }

        [Required]
        public string Address { get; set; }

        [ForeignKey("User")]
        public int UserId { get; set; }
        public User User { get; set; }

        public List<Guest> Guests { get; set; }
        public Wedding()
        {
            // creates an instance of an empty list of guest objects
            Guests = new List<Guest>();
        }
    }
}