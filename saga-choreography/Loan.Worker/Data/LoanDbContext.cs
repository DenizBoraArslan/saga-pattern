using Microsoft.EntityFrameworkCore;

namespace Loan.Worker.Data;

public class LoanDbContext : DbContext
{
    public LoanDbContext(DbContextOptions<LoanDbContext> options) : base(options)
    {
        
    }
    public DbSet<Loan> Loans => Set<Loan>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Loan>(entity =>
        {
            entity.HasKey(l => l.Id);
            entity.Property(l => l.BookId).IsRequired();
            entity.Property(l => l.MemberId).IsRequired();
            entity.Property(l => l.Status).IsRequired();
        });
    }


}

