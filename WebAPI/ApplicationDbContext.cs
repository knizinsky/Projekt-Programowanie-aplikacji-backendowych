using ApplicationCore.Models;
using Infrastructure.EF.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace WebAPI
{
    /// <summary>
    /// Represents the application's database context.
    /// </summary>
    public class ApplicationDbContext : IdentityDbContext<UserEntity, UserRole, int>
    {
        public DbSet<Customer> Customers { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            base.OnConfiguring(optionsBuilder);

            // Configure the database connection
            optionsBuilder.UseSqlServer(
                "Server=ACERASPIRE5\\SQLEXPRESS;Database=Grocery1;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True");
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<Customer>()
                .HasMany<Order>(p => p.Orders)
                .WithOne(m => m.Customer)
                .HasForeignKey(m => m.CustomerId);

            modelBuilder.Entity<Order>()
                .HasMany<OrderItem>(m => m.OrderItems)
                .WithOne(t => t.Order)
                .HasForeignKey(t => t.OrderId);
        }
    }
}