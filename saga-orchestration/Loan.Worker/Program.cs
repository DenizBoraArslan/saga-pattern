
using Contracts.Commands;
using Loan.Worker;
using Loan.Worker.Cosumers;
using Loan.Worker.Data;
using MassTransit;
using Microsoft.EntityFrameworkCore;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddDbContext<LoanDbContext>(options => options.UseSqlServer(builder.Configuration.GetConnectionString("LoanSagaDb")));

builder.Services.AddMassTransit(x =>
{
    x.AddConsumer<CreateLoanCommandConsumer>();

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

builder.Services.AddHostedService<Worker>();

var host = builder.Build();

using (var scope = host.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<LoanDbContext>();
    db.Database.Migrate();
}

host.Run();
