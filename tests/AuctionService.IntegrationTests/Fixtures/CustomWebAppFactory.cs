using AuctionService.Data;
using AuctionService.IntegrationTests.Util;
using MassTransit;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Testcontainers.PostgreSql;
using WebMotions.Fake.Authentication.JwtBearer;

namespace AuctionService.IntegrationTests;

public class CustomWebAppFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgreSqlContainer = new PostgreSqlBuilder().Build();
    public async Task InitializeAsync()
    {
        await _postgreSqlContainer.StartAsync();
    }

    // reconfigure services after initialized from program.cs
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureTestServices(services=> 
        {
            services.RemoveDbContext<AuctionDbContext>();
            // 3. add fake npsql container into services
            services.AddDbContext<AuctionDbContext>(Options => 
            {
                Options.UseNpgsql(_postgreSqlContainer.GetConnectionString());
            });
            
            // replace to test harness, mass transit provide test function
            services.AddMassTransitTestHarness();

            // migrating database with correct schema based on existing migration
            services.EnsureCreated<AuctionDbContext>();

            // declare fake jwt token and would not validate via identity server
            services.AddAuthentication(FakeJwtBearerDefaults.AuthenticationScheme)
                .AddFakeJwtBearer(opt => 
                {
                    opt.BearerValueType = FakeJwtBearerBearerValueType.Jwt;
                });
        });
        base.ConfigureWebHost(builder);

    }
    Task IAsyncLifetime.DisposeAsync() => _postgreSqlContainer.DisposeAsync().AsTask();
}
