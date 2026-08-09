using Loan.Orchestrator;
using Loan.Orchestrator.Data;
using MassTransit;
using Microsoft.EntityFrameworkCore;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddMassTransit(x =>
{
    x.AddSagaStateMachine<LoanStateMachine, LoanSagaState>()
        .EntityFrameworkRepository(r =>
        {
            r.ExistingDbContext<LoanSagaDbContext>();
            r.UseSqlServer();
        });

    x.UsingRabbitMq((context, cfg) =>
    {
        cfg.Host("localhost", 5673, "/", h =>
        {
            h.Username("guest");
            h.Password("guest");
        });

        cfg.ConfigureEndpoints(context);
    });
});

builder.Services.AddDbContext<LoanSagaDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("LoanSagaDb")));

var host = builder.Build();

using (var scope = host.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<LoanSagaDbContext>();
    db.Database.Migrate();
}

host.Run();