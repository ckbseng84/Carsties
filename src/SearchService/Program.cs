using System.Net;
using Polly;
using Polly.Extensions.Http;
using SearchService.Data;
using SearchService.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddHttpClient<AuctionSvcHttpClient>()
    .AddPolicyHandler(GetPolicy()); //retry until success
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
