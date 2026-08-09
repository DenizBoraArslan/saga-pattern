using Microsoft.EntityFrameworkCore;

namespace Catalog.Worker.Data;

public class CatalogDbContext : DbContext
{

    public CatalogDbContext(DbContextOptions<CatalogDbContext> options)
        : base(options)
    {

    }
    public DbSet<Book> Books => Set<Book>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Book>(entity =>
        {
            entity.HasKey(b => b.Id);
            entity.Property(b => b.Title).HasMaxLength(200).IsRequired();
            entity.Property(b => b.StockQuantity).IsRequired();
        });
    }
}

