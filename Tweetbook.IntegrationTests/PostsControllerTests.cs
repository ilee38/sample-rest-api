using System.Net;
using Tweetbook.Contracts.V1;
using Tweetbook.Domain;

namespace Tweetbook.IntegrationTests
{
	public class PostsControllerTests : IClassFixture<TestAppFactory>
	{
		private HttpClient _client;
		private readonly TestAppFactory _factory;

		public PostsControllerTests(TestAppFactory factory)
		{
			_factory = factory;
		}

		[Fact]
		public async Task GetAll_WithoutPosts_ReturnsEmptyResponse()
		{
			// Arrange
			_client = await _factory.RegisterClientAsync();

			// Act
			var response = await _client.GetAsync(ApiRoutes.Posts.GetAll);
			var postsList = await response.Content.ReadFromJsonAsync<List<Post>>();

			// Assert
			Assert.Equal(HttpStatusCode.OK, response.StatusCode);
			Assert.Empty(postsList);
		}


	}
}

