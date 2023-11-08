﻿using Microsoft.AspNetCore.Mvc;
using Tweetbook.Contracts.V1;
using Tweetbook.Contracts.V1.Requests;
using Tweetbook.Contracts.V1.Responses;
using Tweetbook.Domain;
using Tweetbook.Extensions;
using Tweetbook.Services;

namespace Tweetbook.Controllers.V1
{
   public class TagsController : ControllerBase
   {
      private readonly IPostService _postService;

      public TagsController(IPostService postService)
      {
         _postService = postService;
      }

      [HttpGet(ApiRoutes.Tags.GetAll)]
      public async Task<IActionResult> GetAll()
      {
         return Ok(await _postService.GetTagsAsync());
      }

      [HttpPost(ApiRoutes.Tags.Create)]
      public async Task<IActionResult> Create([FromBody] CreateTagRequest request)
      {
            var tag = new Tag
            {
               Name = request.TagName,
               CreatorId = HttpContext.GetUserId(),
               CreatedOn = DateTime.UtcNow
            };

            var created = await _postService.CreateTagAsync(tag);

            if (!created)
            {
               return BadRequest(new { error = "Unable to create tag."});
            }

            var baseUrl = $"{HttpContext.Request.Scheme}://{HttpContext.Request.Host.ToUriComponent()}";
            var locationUri = baseUrl + "/" + ApiRoutes.Tags.Get.Replace("{tagName}", tag.Name);

            var response = new TagResponse { Name = tag.Name };

            return Created(locationUri, response);
      }
   }
}


