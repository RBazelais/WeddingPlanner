
using System;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
 
namespace WeddingPlanner.Models
{
    public class WeddingPlannerContext : DbContext
    {
        // base() calls the parent class' constructor passing the "options" parameter along
        public WeddingPlannerContext(DbContextOptions<WeddingPlannerContext> options) : base(options) { }
        public DbSet<User> Users { get; set; }
        public DbSet<Wedding> Weddings { get; set; }

        public DbSet<Guest> Guests { get; set; }


    }
}