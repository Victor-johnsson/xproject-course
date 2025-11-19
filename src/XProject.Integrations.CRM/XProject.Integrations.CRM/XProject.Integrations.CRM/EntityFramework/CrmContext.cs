using Microsoft.EntityFrameworkCore;
using System;
using XProject.Integrations.CRM.EntityFramework.Entities;

namespace XProject.Integrations.CRM.EntityFramework
{
    public class CrmContext : DbContext
    {
        public CrmContext(DbContextOptions<CrmContext> options) : base(options)
        {

        }

        public DbSet<Customer> Customers { get; set; }
        public DbSet<OrderLine> OrderLines { get; set; }
        public DbSet<Order> Orders { get; set; }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Customers

            modelBuilder.Entity<Customer>()
                .HasKey(c => c.Id);

            modelBuilder.Entity<Customer>()
                .Property(c => c.Id)
                .ValueGeneratedOnAdd();

            modelBuilder.Entity<Customer>()
                .Property(c => c.Name)
                .IsRequired();

            modelBuilder.Entity<Customer>()
                .Property(c => c.Email).IsRequired();

            modelBuilder.Entity<Customer>()
                .Property(c => c.Address).IsRequired();

            // Orders

            modelBuilder.Entity<Order>()
                .HasKey(o => o.Id);

            modelBuilder.Entity<Order>()
                .Property(o => o.Id)
                .ValueGeneratedOnAdd();

            modelBuilder.Entity<Order>()
                .HasOne(o => o.Customer)
                .WithOne(c => c.Order)
                .HasForeignKey<Order>(o => o.CustomerId);

            modelBuilder.Entity<Order>()
                .Property(o => o.Status).IsRequired();

            // OrderLine

            modelBuilder.Entity<OrderLine>()
                .HasKey(o => o.Id);

            modelBuilder.Entity<OrderLine>()
                .HasOne(ol => ol.Order)
                .WithMany(o => o.OrderLines)
                .HasForeignKey(ol => ol.OrderId);

            modelBuilder.Entity<OrderLine>()
                .Property(ol => ol.ProdRef).IsRequired();

            modelBuilder.Entity<OrderLine>()
                .Property(ol => ol.ItemCount).IsRequired();
        }
    }
}