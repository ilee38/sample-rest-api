using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Testcontainers.PostgreSql;
using Tweetbook.Data;

namespace Tweetbook.IntegrationTests
{
    public class TestAppFactory : WebApplicationFactory<Program>, IAsyncLifetime
    {
        protected readonly PostgreSqlContainer _postgreSqlContainer;

        public TestAppFactory()
        {
            _postgreSqlContainer = new PostgreSqlBuilder().Build();
        }
      
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.ConfigureServices(services =>
				    {
                var dbContextDescriptor = services.SingleOrDefault(d =>
                    d.ServiceType == typeof(DbContextOptions<DataContext>));

                if (dbContextDescriptor != null)
                {
                    services.Remove(dbContextDescriptor);
                }

					      var connectionString = _postgreSqlContainer.GetConnectionString();
                services.AddDbContext<DataContext>(options =>
                {
                    options.UseNpgsql(connectionString);
                });

                var serviceProvider = services.BuildServiceProvider();
                using (var scope = serviceProvider.CreateScope())
                {
                    var scopedServices = scope.ServiceProvider;
                    var context = scopedServices.GetRequiredService<DataContext>();
                    context.Database.EnsureCreated();
                }
            });
        }

				public Task InitializeAsync()
				{
				  return _postgreSqlContainer.StartAsync();

				}

				public Task DisposeAsync()
				{
				  return _postgreSqlContainer.DisposeAsync().AsTask();
				}
    }
}

