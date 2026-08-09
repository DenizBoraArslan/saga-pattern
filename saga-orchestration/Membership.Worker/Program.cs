using MassTransit;
using Membership.Worker;
using Membership.Worker.Consumers;
using Membership.Worker.Data;
using Microsoft.EntityFrameworkCore;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddDbContext<MembershipDbContext>(options => options.UseSqlServer(builder.Configuration.GetConnectionString("MembershipDb")));

builder.Services.AddMassTransit(x =>
{
    x.AddConsumer<VerifyMembershipCommandConsumer>();

    x.UsingRabbitMq((context, cfg) =>
    {
        cfg.Host("localhost", 5673,"/", h =>
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
    var db = scope.ServiceProvider.GetRequiredService<MembershipDbContext>();
    db.Database.Migrate();

    if (!db.Members.Any())
    {
        db.Members.AddRange(
            new Member { Id = Guid.NewGuid(), Name = "Ahmet Yýlmaz", IsEligible = true, OverdueBooksCount = 0 },
            new Member { Id = Guid.NewGuid(), Name = "Ayþe Demir", IsEligible = false, OverdueBooksCount = 3 }
        );
        db.SaveChanges();
    }
}

host.Run();
