using Microsoft.EntityFrameworkCore;

namespace Loan.Worker.Data;

public class LoanDbContext : DbContext
{
    public LoanDbContext(DbContextOptions<LoanDbContext> options) : base(options)
    {
        
    }

    public DbSet<Loan> Loan => Set<Loan>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Loan>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.BookId).IsRequired();
            entity.Property(e => e.MemberId).IsRequired();
            entity.Property(e => e.Status).IsRequired();
        });
    }

}

