using System.Net;
using System.Net.Http.Headers;
using Org.BouncyCastle.Tls;
using Tweetbook.Contracts.V1;
using Tweetbook.Contracts.V1.Requests;
using Tweetbook.Contracts.V1.Responses;
using Tweetbook.Domain;

namespace Tweetbook.IntegrationTests
{
		public class PostsControllerTests : IClassFixture<TestAppFactory>
		{
				private readonly HttpClient _client;
				private readonly TestAppFactory _factory;
		
				public PostsControllerTests(TestAppFactory factory)
				{ 
						_factory = factory;
						_client = _factory.CreateClient();
				}

				[Fact]
				public async Task GetAll_WithoutPosts_ReturnsEmptyResponse() 
				{
						// Arrange
						var authResponse = _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", await GetJwtAsync());

						// Act
						var response = await _client.GetAsync(ApiRoutes.Posts.GetAll);

						// Assert
						Assert.Equal(HttpStatusCode.OK, response.StatusCode);
						Assert.Empty(await response.Content.ReadFromJsonAsync<List<Post>>());
				}

				private async Task<string> GetJwtAsync()
				{
						var response = await _client.PostAsJsonAsync(ApiRoutes.Identity.Register, new UserRegistrationRequest
						{
								Email = "test1@integration.com",
								Password = "SomePass1234!"
						});

						var registrationResponse = await response.Content.ReadFromJsonAsync<AuthSuccessResponse>();
						return registrationResponse.Token;
				}
		}
}

