using DigitalStore.Core.Models;
using DigitalStore.Repository.Configurations;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace DigitalStore.Repository.Context
{
    public class ApplicationDbContext : IdentityDbContext<User, IdentityRole<int>, int>
    {
        public ApplicationDbContext(DbContextOptions options) : base(options)
        {
        }

        public DbSet<Contact> Contacts { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<Subject> Subjects { get; set; }
        public DbSet<Coupon> Coupons { get; set; }
        public DbSet<PasswordReset> PasswordResets { get; set; }
        public DbSet<Category> Categories { get; set; } = null!; // Kategoriler için DbSet
        public DbSet<ProductCategory> ProductCategories { get; set; } = null!; // Junction Table

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // ProductCategory için Many-to-Many ilişki yapılandırması
            modelBuilder.Entity<ProductCategory>()
                .HasKey(pc => new { pc.ProductId, pc.CategoryId }); // Composite key tanımlama

            modelBuilder.Entity<ProductCategory>()
                .HasOne(pc => pc.Product)
                .WithMany(p => p.ProductCategories)
                .HasForeignKey(pc => pc.ProductId); // Product ile ilişki

            modelBuilder.Entity<ProductCategory>()
                .HasOne(pc => pc.Category)
                .WithMany(c => c.ProductCategories)
                .HasForeignKey(pc => pc.CategoryId); // Category ile ilişki

            // Diğer konfigürasyonlar
            modelBuilder.ApplyConfiguration(new ProductCategoryConfiguration());
        }
    }
}
