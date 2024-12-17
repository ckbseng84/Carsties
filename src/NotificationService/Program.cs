using MassTransit;
using NotificationService.Consumers;
using NotificationService.Hubs;

var builder = WebApplication.CreateBuilder(args);
//add mass transit
builder.Services.AddMassTransit(x=> 
{
    x.AddConsumersFromNamespaceContaining<AuctionCreatedConsumer>();
    // set name for bids in rabbitmq
    x.SetEndpointNameFormatter(new KebabCaseEndpointNameFormatter("nt", false));
    //using rabbit mq
    x.UsingRabbitMq((context, cfg)=> 
    {
        //todo: make it configurable in appsettings
        cfg.Host(builder.Configuration["RabbitMq:Host"],"/",host =>
        {
            host.Username(builder.Configuration.GetValue("RabbitMq:Username","guest"));
            host.Password(builder.Configuration.GetValue("RabbitMq:Password","guest"));

        });
        cfg.ConfigureEndpoints(context);
    });
});
//add signalR
builder.Services.AddSignalR();


var app = builder.Build();

app.MapHub<NotificationHub>("/notifications");

app.Run();
