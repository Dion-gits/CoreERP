using System;
using System.Collections.Generic;
using System.Text;
using Hardware.domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Hardware.infrastructure.Data
{
    public class TenantErpDbContext : DbContext
    {
        public TenantErpDbContext(DbContextOptions<TenantErpDbContext> options) : base(options) { }

        public DbSet<Product> Products => Set<Product>();
        public DbSet<Customer> Customers => Set<Customer>();
        public DbSet<Supplier> Suppliers => Set<Supplier>();
        public DbSet<Inventory> Inventories => Set<Inventory>();
        public DbSet<Sale> Sales => Set<Sale>();
        public DbSet<SaleItem> SaleItems => Set<SaleItem>();

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Product Entity Rules
            builder.Entity<Product>(entity =>
            {
                entity.HasKey(x => x.ProductId);
                entity.Property(x => x.ProductCode).HasMaxLength(50).IsRequired();
                entity.Property(x => x.ProductName).HasMaxLength(200).IsRequired();
                entity.Property(x => x.UnitPrice).HasPrecision(18, 2);
                entity.HasIndex(x => x.ProductCode).IsUnique();
            });

            // Inventory Entity Rules
            builder.Entity<Inventory>()
                .Property(x => x.QuantityOnHand)
                .HasPrecision(18, 2);

            builder.Entity<Inventory>()
                .Property(x => x.ReorderLevel)
                .HasPrecision(18, 2);

            // Sales & SaleItems Entity Rules
            builder.Entity<Sale>()
                .Property(x => x.TotalAmount)
                .HasPrecision(18, 2);

            builder.Entity<SaleItem>()
                .Property(x => x.Quantity)
                .HasPrecision(18, 2);

            builder.Entity<SaleItem>()
                .Property(x => x.UnitPrice)
                .HasPrecision(18, 2);

            builder.Entity<SaleItem>()
                .Property(x => x.SubTotal)
                .HasPrecision(18, 2);
        }
    }
}