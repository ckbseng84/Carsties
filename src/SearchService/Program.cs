using System.Net;
using MassTransit;
using Polly;
using Polly.Extensions.Http;
using SearchService.Consumers;
using SearchService.Data;
using SearchService.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());
builder.Services.AddHttpClient<AuctionSvcHttpClient>()
    .AddPolicyHandler(GetPolicy()); //retry until success
builder.Services.AddMassTransit(x=> 
{
    //must declare this namespace
    x.AddConsumersFromNamespaceContaining<AuctionCreatedConsumer>();
    x.SetEndpointNameFormatter(new KebabCaseEndpointNameFormatter("search", false));
    //using rabbit mq
    x.UsingRabbitMq((context, cfg)=> 
    {
        cfg.Host(builder.Configuration["RabbitMq:Host"],"/",host =>
        {
            host.Username(builder.Configuration.GetValue("RabbitMq:Username","guest"));
            host.Password(builder.Configuration.GetValue("RabbitMq:Password","guest"));

        });
        cfg.ReceiveEndpoint("search-auction-created", e=> 
        {
            //retry for 5 times with 5 second interval
            e.UseMessageRetry(r => r.Interval(5,5));
            //must declare AddCOnsumersFromNamespaceContaining<AuctionCreatedConsumer>
            e.ConfigureConsumer<AuctionCreatedConsumer>(context);
        });
        cfg.ConfigureEndpoints(context);
    });
});
var app = builder.Build();

// Configure the HTTP request pipeline.

app.UseAuthorization();

app.MapControllers();

//register the action after app.run is complete
app.Lifetime.ApplicationStarted.Register(async () => 
{
    //initialize MongoDB
    try
    {
        await DbInitializer.InitDb(app);
    }
    catch (System.Exception ex)
    {
        Console.WriteLine(ex.Message);
    }
});

app.Run();

//retry policy
static IAsyncPolicy<HttpResponseMessage> GetPolicy() 
            => HttpPolicyExtensions
                .HandleTransientHttpError()
                .OrResult(msg => msg.StatusCode == HttpStatusCode.NotFound)
                .WaitAndRetryForeverAsync(_ => TimeSpan.FromSeconds(3));
