using DigitalStore.Core.Models;
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
        public DbSet<Order> Orders { get; set; }
        public DbSet<PasswordReset> PasswordResets { get; set; }
        public DbSet<Category> Categories { get; set; } = null!;
        public DbSet<ProductCategory> ProductCategories { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Assembly'deki tüm yapılandırmaları uygular
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);

            // Özellikle decimal alanlar için hassasiyet ayarlarını yapın
            modelBuilder.Entity<Product>(entity =>
            {
                entity.Property(e => e.Price).HasColumnType("decimal(18,2)");
                entity.Property(e => e.PointsPercentage).HasColumnType("decimal(5,2)");
                entity.Property(e => e.MaxPoints).HasColumnType("decimal(18,2)");
            });

            modelBuilder.Entity<Order>(entity =>
            {
                entity.Property(e => e.TotalAmount).HasColumnType("decimal(18,2)");
            });

            modelBuilder.Entity<Coupon>(entity =>
            {
                entity.Property(e => e.DiscountAmount).HasColumnType("decimal(18,2)");
            });

            modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
        }
    }
}
