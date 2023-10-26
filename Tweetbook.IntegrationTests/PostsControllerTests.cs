using System.Net;
using Tweetbook.Contracts.V1;
using Tweetbook.Contracts.V1.Requests;
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
		public async Task GetAll_WithExistingPosts_ReturnsNonEmptyResponse()
		{
			// Arrange
			_client = await _factory.RegisterClientAsync();

			// create post in DB.
			string postName = "Some test post.";
			var postResponse = await _client.PostAsJsonAsync(ApiRoutes.Posts.Create, new CreatePostRequest { Name = postName });

			// Act
			var response = await _client.GetAsync(ApiRoutes.Posts.GetAll);
			var postsList = await response.Content.ReadFromJsonAsync<List<Post>>();

			// Assert
			Assert.Equal(HttpStatusCode.OK, response.StatusCode);
			Assert.NotEmpty(postsList);
		}

		[Fact]
		public async Task Get_ReturnsPost_WhenPostExistsInDb()
		{
			// Arrange
			_client = await _factory.RegisterClientAsync();

			// create new post in DB.
			string postName = "Some test post.";
			var postResponse = await _client.PostAsJsonAsync(ApiRoutes.Posts.Create, new CreatePostRequest { Name = postName });
      	var createdPost = await postResponse.Content.ReadFromJsonAsync<Post>();

			// Act
			var response = await _client.GetAsync(ApiRoutes.Posts.Get.Replace("{postId}", createdPost.Id.ToString()));
			var returnedPost = await response.Content.ReadFromJsonAsync<Post>();

			// Assert
			Assert.Equal(HttpStatusCode.OK, response.StatusCode);
			Assert.Equal(createdPost.Id, returnedPost.Id);
			Assert.Equal(postName, returnedPost.Name);
		}
	}
}

