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
            post.Tags?.ForEach(x => x.TagName = x.TagName.ToLower());

            await AddNewTags(post);
            await _dataContext.Posts.AddAsync(post);
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
            // First, we need to perform a join between Posts table and PostTags table to get all tags
            // associated with each post.
            var queryable = _dataContext.Posts.AsQueryable();

            return await queryable.Include(x => x.Tags).ToListAsync();
        }

        public async Task<bool> UpdatePostAsync(Post postToUpdate)
        {
            _dataContext.Posts.Update(postToUpdate);
            var updated = await _dataContext.SaveChangesAsync();

            return updated > 0;
        }

        public async Task<bool> UserOwnsPostAsync(Guid postId, string userId)
        {
            var post = await _dataContext.Posts.AsNoTracking().SingleOrDefaultAsync(x => x.Id == postId);

            if (post == null)
            {
                return false;
            }

            if (post.UserId != userId)
            {
                return false;
            }

            return true;
        }

        public async Task<List<Tag>> GetTagsAsync()
        {
            return await _dataContext.Tags.ToListAsync();
        }

        public async Task<bool> CreateTagAsync(Tag tag)
        {
            _dataContext.Tags.Add(tag);
            var created = await _dataContext.SaveChangesAsync();

            return created > 0;
        }

        private async Task AddNewTags(Post post)
        {
            foreach (var tag in post.Tags)
            {
                var exsistingTag = await _dataContext.Tags.SingleOrDefaultAsync(x => x.Name == tag.TagName);
                if (exsistingTag != null)
                {
                    continue;
                }

                await _dataContext.Tags.AddAsync(new Tag {
                    Name = tag.TagName,
                    CreatedOn = DateTime.UtcNow,
                    CreatorId = post.UserId
                });
            }
        }
    }
}
