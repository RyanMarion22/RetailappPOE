using Microsoft.EntityFrameworkCore;
using RetailappPOE.Models;
using RetailappPOE.Models.SQLModels;

namespace RetailappPOE.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options) { }

        public DbSet<User> Users { get; set; } = null!;
        public DbSet<ProductSQL> Products { get; set; } = null!;
        public DbSet<OrderSQL> Orders { get; set; } = null!;
        public DbSet<CartItem> CartItems { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // ---------- Primary Keys ----------
            modelBuilder.Entity<User>()
                .Property(u => u.Id).ValueGeneratedOnAdd();

            modelBuilder.Entity<ProductSQL>()
                .Property(p => p.Id).ValueGeneratedOnAdd();

            modelBuilder.Entity<OrderSQL>()
                .Property(o => o.Id).ValueGeneratedOnAdd();

            modelBuilder.Entity<CartItem>()
                .Property(c => c.Id).ValueGeneratedOnAdd();

            // ---------- Fix decimal precision ----------
            modelBuilder.Entity<ProductSQL>()
                .Property(p => p.Price)
                .HasColumnType("decimal(18,2)");

            // ---------- RELATIONSHIPS (NO FOREIGN KEY TO USERS) ----------
            // REMOVED: FK_Orders_Users_CustomerId → NO USER DEPENDENCY

            modelBuilder.Entity<OrderSQL>()
                .HasMany(o => o.Items)
                .WithOne(c => c.Order)
                .HasForeignKey(c => c.OrderId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<CartItem>()
                .HasOne(c => c.Product)
                .WithMany()
                .HasForeignKey(c => c.ProductId)
                .OnDelete(DeleteBehavior.Cascade);

            // ---------- ALLOW CustomerId = 0 (ANONYMOUS CART) ----------
            modelBuilder.Entity<OrderSQL>()
                .Property(o => o.CustomerId)
                .HasDefaultValue(0); // Allow guest cart

            // ---------- OrderDate NULLABLE ----------
            modelBuilder.Entity<OrderSQL>()
                .Property(o => o.OrderDate)
                .IsRequired(false); // NULL until checkout
        }
    }
}