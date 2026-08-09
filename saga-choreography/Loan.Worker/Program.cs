using Loan.Worker;
using Loan.Worker.Consumers;
using Loan.Worker.Data;
using MassTransit;
using Microsoft.EntityFrameworkCore;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddDbContext<LoanDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("LoanDb")));

builder.Services.AddMassTransit(x =>
{
    x.AddConsumer<MembershipVerificationFailedConsumer>();
    x.AddConsumer<StockReservationFailedConsumer>();
    x.AddConsumer<StockReservedConsumer>();

    x.UsingRabbitMq((context, cfg) =>
    {
        cfg.Host("localhost", "/", h =>
        {
            h.Username("guest");
            h.Password("guest");
        });

        cfg.ConfigureEndpoints(context);
    });
});

var host = builder.Build();

using (var scope = host.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<LoanDbContext>();
    db.Database.Migrate();
}

host.Run();