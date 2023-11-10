using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Tweetbook.Contracts.V1;
using Tweetbook.Contracts.V1.Requests;
using Tweetbook.Contracts.V1.Responses;
using Tweetbook.Domain;
using Tweetbook.Extensions;
using Tweetbook.Services;

namespace Tweetbook.Controllers.V1
{
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class PostsController : ControllerBase
    {
        private readonly IPostService _postService;

        public PostsController(IPostService postService)
        {
            _postService = postService;
        }

        [HttpGet(ApiRoutes.Posts.GetAll)]
        public async Task<IActionResult> GetAll()
        {
            var posts = await _postService.GetPostsAsync();
            var postResponses = posts.Select(post => new PostResponse
            {
                Id = post.Id,
                Name = post.Name,
                UserId = post.UserId,
                Tags = post.Tags.Select(x => new TagResponse{ Name = x.TagName }).ToList()
            }).ToList();

            return Ok(postResponses);
        }

        [HttpGet(ApiRoutes.Posts.Get)]
        public async Task<IActionResult> Get([FromRoute] Guid postId)
        {
            var post = await _postService.GetPostByIdAsync(postId);

            if (post == null)
            {
                return NotFound();
            }

            var response = new PostResponse {
                Id = post.Id,
                Name = post.Name,
                UserId = post.UserId,
                Tags = post.Tags.Select(x => new TagResponse{ Name = x.TagName })
            };

            return Ok(response);
        }

        [HttpPost(ApiRoutes.Posts.Create)]
        public async Task<IActionResult> Create([FromBody] CreatePostRequest postRequest)
        {
            // Here we're mapping the request object to the our domain object Post.
            // This is a common practice to avoid exposing our domain objects to the outside world, and create a nice separation.
            // It also allows us to work with versioning without affecting the outside world.
            var newPostId = Guid.NewGuid();
            var post = new Post
            {
                Id = newPostId,
                Name = postRequest.Name,
                UserId = HttpContext.GetUserId(),
                Tags = new List<PostTag>() {}
            };

            if (postRequest.Tags.Count() > 0)
            {
                foreach (var tag in postRequest.Tags)
                {
                    post.Tags.Add(new PostTag
                    {
                        TagName = tag,
                        PostId = newPostId
                    });
                }
            }

            await _postService.CreatePostAsync(post);

            var baseUrl = $"{HttpContext.Request.Scheme}://{HttpContext.Request.Host.ToUriComponent()}";
            var locationUri = baseUrl + "/" + ApiRoutes.Posts.Get.Replace("{postId}", post.Id.ToString());

            // The response is also versioned. So we map it to a PostResponse contract.
            var response = new PostResponse {
                Id = post.Id,
                Name = post.Name,
                UserId = post.UserId,
                Tags = post.Tags.Select(x => new TagResponse{ Name = x.TagName })
            };

            return Created(locationUri, response);
        }

        [HttpPut(ApiRoutes.Posts.Update)]
        public async Task<IActionResult> Update([FromRoute] Guid postId, [FromBody] UpdatePostRequest request)
        {
            var userOwnsPost = await _postService.UserOwnsPostAsync(postId, HttpContext.GetUserId());

            if (!userOwnsPost)
            {
                return BadRequest(new { error = "You don't own this post." });
            }

            var post = await _postService.GetPostByIdAsync(postId);
            post.Name = request.Name;

            var updated = await _postService.UpdatePostAsync(post);

            if (updated)
            {
                var response = new PostResponse
                {
                    Id = post.Id,
                    Name = post.Name,
                    UserId = post.UserId,
                    Tags = post.Tags.Select(x => new TagResponse{ Name = x.TagName })
                };
                return Ok(response);
            }

            return NotFound();
        }

        [HttpDelete(ApiRoutes.Posts.Delete)]
        public async Task<IActionResult> Delete([FromRoute] Guid postId)
        {
            var userOwnsPost = await _postService.UserOwnsPostAsync(postId, HttpContext.GetUserId());

            if (!userOwnsPost)
            {
                return BadRequest(new { error = "You don't own this post." });
            }

            var deleted = await _postService.DeletePostAsync(postId);

            if (deleted)
            {
                return NoContent();
            }

            return NotFound();
        }
    }
}
