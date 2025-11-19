using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ValueGeneration;
using Models;

public class PimDbContext : DbContext
{
    public PimDbContext(DbContextOptions<PimDbContext> options)
        : base(options) { }

    public DbSet<Product> Products { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultContainer("Products");

        _ = modelBuilder.Entity<Product>(e =>
        {
            e.Property(e => e.Id).HasValueGenerator<GuidValueGenerator>();
            e.HasPartitionKey(e => e.PartitionKey);
            e.ToContainer("Products");
            e.Property<long>("_ts"); // Allows querying _ts as a long (Unix timestamp)
        });
    }
}
