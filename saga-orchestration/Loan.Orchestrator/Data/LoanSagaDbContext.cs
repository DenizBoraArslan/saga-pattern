using MassTransit;
using MassTransit.EntityFrameworkCoreIntegration;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Loan.Orchestrator.Data;

public class LoanSagaDbContext : SagaDbContext
{
    public LoanSagaDbContext(DbContextOptions<LoanSagaDbContext> options) : base(options) { }

    protected override IEnumerable<ISagaClassMap> Configurations
    {
        get { yield return new LoanSagaStateMap(); }
    }
}

public class LoanSagaStateMap : SagaClassMap<LoanSagaState>
{
    protected override void Configure(EntityTypeBuilder<LoanSagaState> entity, ModelBuilder model)
    {
        entity.Property(x => x.CurrentState).HasMaxLength(64);
        entity.Property(x => x.FailureReason).HasMaxLength(500).IsRequired(false);
        entity.Property(x => x.Version).IsConcurrencyToken();
    }
}