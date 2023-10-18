using Microsoft.EntityFrameworkCore;
using Tweetbook.Data;
using Tweetbook.Domain;

namespace Tweetbook.Services
{
    /// <summary>
    /// Post service class to initialize our DataContext and use it as a Scoped service (see Program.cs)
    /// </summary>
    public class PostService : IPostService
    {
        private readonly DataContext _dataContext; 

        public PostService(DataContext dataContext)
        {
            _dataContext = dataContext;
        }

        public async Task<bool> CreatePostAsync(Post post)
        {
            _dataContext.Posts.Add(post);
            var created = await _dataContext.SaveChangesAsync();

            return created > 0;
        }

        public async Task<bool> DeletePostAsync(Guid postId)
        {
            var post = await GetPostByIdAsync(postId);

            if (post == null)
            {
                return false;
            }

            _dataContext.Posts.Remove(post);
            var deleted = await _dataContext.SaveChangesAsync();

            return deleted > 0;
        }

        public async Task<Post> GetPostByIdAsync(Guid postId)
        {
            return await _dataContext.Posts.SingleOrDefaultAsync(x => x.Id == postId);
        }

        public async Task<List<Post>> GetPostsAsync()
        {
            return await _dataContext.Posts.ToListAsync();
        }

        public async Task<bool> UpdatePostAsync(Post postToUpdate)
        {
            _dataContext.Posts.Update(postToUpdate);
            var updated = await _dataContext.SaveChangesAsync();

            return updated > 0;
        }
    }
}
