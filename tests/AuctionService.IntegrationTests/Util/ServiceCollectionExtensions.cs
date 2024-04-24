using AuctionService.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace AuctionService.IntegrationTests.Util
{
    public static class ServiceCollectionExtensions
    {
        public static void RemoveDbContext<T>(this IServiceCollection services)
        {
            // replace db connection with test container
            // 1. get auction db context
            var descriptor = services.SingleOrDefault(d=> d.ServiceType == typeof(DbContextOptions<AuctionDbContext>));
            // 2. if db context is null then remove from the services
            if (descriptor != null) services.Remove(descriptor);
        }
        public static void EnsureCreated<T>(this IServiceCollection services)
        {
            var sp = services.BuildServiceProvider();
            using var scope =sp.CreateScope();
            var scopedServices = scope.ServiceProvider;
            var db = scopedServices.GetRequiredService<AuctionDbContext>();
            db.Database.Migrate();
            DBHelper.InitDbForTests(db);

        }
    }
}