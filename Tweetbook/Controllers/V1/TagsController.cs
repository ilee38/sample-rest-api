﻿using AutoMapper;
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
   [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Admin, Poster")]
   public class TagsController : ControllerBase
   {
      private readonly IPostService _postService;
      private readonly IMapper _mapper;

      public TagsController(IPostService postService, IMapper mapper)
      {
         _postService = postService;
         _mapper = mapper;
      }

      [HttpGet(ApiRoutes.Tags.GetAll)]
      public async Task<IActionResult> GetAll()
      {
         var tags = await _postService.GetTagsAsync();
         return Ok(new PagedResponse<TagResponse>(_mapper.Map<List<TagResponse>>(tags)));
      }

      [HttpGet(ApiRoutes.Tags.Get)]
      public async Task<IActionResult> Get([FromRoute] string tagName)
      {
         var tag = await _postService.GetTagByNameAsync(tagName);

         if (tag == null)
         {
            return NotFound();
         }

         return Ok(new Response<TagResponse>(_mapper.Map<TagResponse>(tag)));
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

         return Created(locationUri, new Response<TagResponse>(_mapper.Map<TagResponse>(tag)));
      }

      // Only users with role = Admin can delete tags.
      [HttpDelete(ApiRoutes.Tags.Delete)]
      [Authorize(Roles = "Admin")]
      public async Task<IActionResult> Delete([FromRoute] string tagName)
      {
         var deleted = await _postService.DeleteTagAsync(tagName);

         if (deleted)
         {
            return NoContent();
         }

         return NotFound();
      }
   }
}


