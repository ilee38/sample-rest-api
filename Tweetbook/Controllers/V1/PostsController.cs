using AutoMapper;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Tweetbook.Cache;
using Tweetbook.Contracts.V1;
using Tweetbook.Contracts.V1.Requests;
using Tweetbook.Contracts.V1.Requests.Queries;
using Tweetbook.Contracts.V1.Responses;
using Tweetbook.Domain;
using Tweetbook.Extensions;
using Tweetbook.Helpers;
using Tweetbook.Services;

namespace Tweetbook.Controllers.V1
{
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class PostsController : ControllerBase
    {
        private readonly IPostService _postService;
        private readonly IMapper _mapper;
        private readonly IUriService _uriService;

        public PostsController(IPostService postService, IMapper mapper, IUriService uriService)
        {
            _postService = postService;
            _mapper = mapper;
            _uriService = uriService;
        }

        [HttpGet(ApiRoutes.Posts.GetAll)]
        [Cached(600)]
        public async Task<IActionResult> GetAll([FromQuery]PaginationQuery paginationQuery)
        {
            var paginationFilter = _mapper.Map<PaginationFilter>(paginationQuery);
            var posts = await _postService.GetPostsAsync(paginationFilter);
            var postsResponse = _mapper.Map<List<PostResponse>>(posts);

            if (paginationFilter == null || paginationFilter.PageNumber < 1 || paginationFilter.PageSize < 1)
            {
                return Ok(new PagedResponse<PostResponse>(postsResponse));
            }

            var paginationResponse = PaginationHelpers.CreatePaginatedResponse(_uriService, paginationFilter, postsResponse);

            return Ok(paginationResponse);
        }

        [HttpGet(ApiRoutes.Posts.Get)]
        [Cached(600)]
        public async Task<IActionResult> Get([FromRoute] Guid postId)
        {
            var post = await _postService.GetPostByIdAsync(postId);

            if (post == null)
            {
                return NotFound();
            }

            return Ok(new Response<PostResponse>(_mapper.Map<PostResponse>(post)));
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

            var locationUri = _uriService.GetPostUri(post.Id.ToString());

            return Created(locationUri, new Response<PostResponse>(_mapper.Map<PostResponse>(post)));
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

            if (!updated)
            {
                return NotFound();
            }

            return Ok(new Response<PostResponse>(_mapper.Map<PostResponse>(post)));
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
