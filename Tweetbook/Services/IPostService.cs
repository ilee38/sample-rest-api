using Tweetbook.Domain;

namespace Tweetbook.Services
{
    public interface IPostService
    {
        Task<bool> CreatePostAsync(Post post);

        Task<List<Post>> GetPostsAsync();

        Task<Post> GetPostByIdAsync(Guid postId);

        Task<bool> UpdatePostAsync(Post postToUpdate);

        Task<bool> DeletePostAsync(Guid postId);

        Task<bool> UserOwnsPostAsync(Guid postId, string userId);

        Task<List<Tag>> GetTagsAsync();

        Task<bool> CreateTagAsync(Tag tag);
    }
}
