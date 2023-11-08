using Tweetbook.Domain;

namespace Tweetbook.Contracts.V1.Requests
{
    public class CreatePostRequest
    {
        public string Name { get; set; }

        public IEnumerable<string> Tags { get; set; }
    }
}
