using Catalog.Worker;
using Catalog.Worker.Data;
using Microsoft.EntityFrameworkCore;
using MassTransit;
using Catalog.Worker.Consumers;


var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddDbContext<CatalogDbContext>(options => options.UseSqlServer(builder.Configuration.GetConnectionString("CatalogDb")));

builder.Services.AddMassTransit(x =>
{

    x.AddConsumer<MembershipVerifiedConsumer>();
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
    var db = scope.ServiceProvider.GetRequiredService<CatalogDbContext>();
    db.Database.Migrate();
    if (!db.Books.Any())
    {
        db.Books.AddRange(
            new Book { Id = Guid.NewGuid(), StockQuantity =5,  Title= "Yüzüklerin Efendisi" },
            new Book { Id = Guid.NewGuid(), StockQuantity =10, Title= "Harry Potter" }
        );
        db.SaveChanges();
    }
}

host.Run();
