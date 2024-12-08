using AuctionService;
using AuctionService.Consumers;
using AuctionService.Data;
using MassTransit;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddDbContext<AuctionDbContext>(options => 
{
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"));
});

builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

//add mass transit
builder.Services.AddMassTransit(x=> 
{
    x.AddEntityFrameworkOutbox<AuctionDbContext>(o =>
    {
        o.QueryDelay = TimeSpan.FromSeconds(10);
        o.UsePostgres();
        o.UseBusOutbox();
    });
    x.AddConsumersFromNamespaceContaining<AuctionCreateFaultConsumer>();
    x.SetEndpointNameFormatter(new KebabCaseEndpointNameFormatter("auction", false));
    //using rabbit mq
    x.UsingRabbitMq((context, cfg)=> 
    {
        cfg.Host(builder.Configuration["RabbitMq:Host"],"/",host =>
        {
            host.Username(builder.Configuration.GetValue("RabbitMq:Username","guest"));
            host.Password(builder.Configuration.GetValue("RabbitMq:Password","guest"));

        });
        cfg.ConfigureEndpoints(context);
    });
});
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.Authority = builder.Configuration["IdentityServiceUrl"];
        //current identity server is set for http
        options.RequireHttpsMetadata = false;
        options.TokenValidationParameters.ValidateAudience = false;
        options.TokenValidationParameters.NameClaimType = "username";
    });
builder.Services.AddScoped<IAuctionRepository, AuctionRepository>();
var app = builder.Build();
//must declare before authorization
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

try
{
    DbInitializer.InitDb(app);
}
catch (System.Exception ex)
{
    Console.WriteLine(ex.Message);
}
app.Run();

public partial class Program{}
 