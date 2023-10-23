using System.Net;
using Tweetbook.Contracts.V1;
using Tweetbook.Domain;

namespace Tweetbook.IntegrationTests
{
		public class PostsControllerTests : IntegrationTest
		{
				[Fact]
				public async Task GetAll_WithoutPosts_ReturnsEmptyResponse() 
				{
						// Arrange
						await AuthenticateAsync();

						// Act
						var response = await TestClient.GetAsync(ApiRoutes.Posts.GetAll);

						// Assert
						Assert.Equal(HttpStatusCode.OK, response.StatusCode);
						Assert.Empty(await response.Content.ReadFromJsonAsync<List<Post>>());

				}
		}
}

