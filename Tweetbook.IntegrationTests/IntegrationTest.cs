using System.Net.Http.Headers;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Testcontainers.PostgreSql;
using Tweetbook.Contracts.V1;
using Tweetbook.Contracts.V1.Requests;
using Tweetbook.Contracts.V1.Responses;
using Tweetbook.Data;

namespace Tweetbook.IntegrationTests
{
		public class IntegrationTest : IAsyncLifetime
		{
				protected readonly HttpClient TestClient;
				protected readonly PostgreSqlContainer _postgreSqlContainer;

				public IntegrationTest()
				{
						_postgreSqlContainer = new PostgreSqlBuilder().Build();

						var appFactory = new WebApplicationFactory<Program>()
								.WithWebHostBuilder(builder =>
								{
										builder.ConfigureServices(async services =>
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
										});
								});

						TestClient = appFactory.CreateClient();
				}

				public async Task InitializeAsync()
				{ 
						await _postgreSqlContainer.StartAsync();
						
				}

				public async Task DisposeAsync()
				{
						await _postgreSqlContainer.DisposeAsync().AsTask();
				}

				protected async Task AuthenticateAsync()
				{
						TestClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", await GetJwtAsync());
				}

				private async Task<string> GetJwtAsync()
				{
						var response = await TestClient.PostAsJsonAsync(ApiRoutes.Identity.Register, new UserRegistrationRequest{
							Email = "test@integration.com",
							Password = "SomePass1234!"
						});

						var registrationResponse = await response.Content.ReadFromJsonAsync<AuthSuccessResponse>();
						return registrationResponse.Token;
				}
		}
}
