using System.Net.Http.Headers;
using Tweetbook.Contracts.V1;
using Tweetbook.Contracts.V1.Requests;
using Tweetbook.Contracts.V1.Responses;

namespace Tweetbook.IntegrationTests
{
		  public static class TestAppFactoryExtensions
		  {
					 private static HttpClient _client;

					 public static async Task<HttpClient> RegisterClientAsync(this TestAppFactory factory)
					 { 
						  _client = factory.CreateClient();
						  _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", await GetJwtAsync());
						  
						  return _client;
					 }

					 private static async Task<string> GetJwtAsync()
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

