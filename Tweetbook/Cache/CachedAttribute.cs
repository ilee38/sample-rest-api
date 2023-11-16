using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Tweetbook.Services;

namespace Tweetbook.Cache
{
   /// <summary>
   /// This middleware attribute class is used to define caching in our controllers (similar to the ApiKeyAuthAttribute class).
   /// </summary>
   [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
   public class CachedAttribute : Attribute, IAsyncActionFilter
   {
      private readonly int _timeToLiveSeconds;

      public CachedAttribute(int timeToLiveSeconds)
      {
         _timeToLiveSeconds = timeToLiveSeconds;
      }

      public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
      {
         // before the controller
         // check if response value is in cache and return it
         var cacheSettings = context.HttpContext.RequestServices.GetRequiredService<RedisCacheSettings>();

         if (!cacheSettings.Enabled)
         {
            await next();
            return;
         }

         var cacheService = context.HttpContext.RequestServices.GetRequiredService<IResponseCacheService>();

         var cacheKey = GenerateCacheKeyFromRequest(context.HttpContext.Request);
         var cachedReponse = await cacheService.GetCacheResponseAsync(cacheKey);

         // TODO: implement the result for when the cachedResponse in null or empty
         if (!string.IsNullOrEmpty(cachedReponse))
         {
            var contentResult = new ContentResult
            {
               Content = cachedReponse,
               ContentType = "application/json",
               StatusCode = 200
            };
            context.Result = contentResult;
            return;
         }

         // In this case, since we want to cache the result for next time, we get the response from the controller
         // then cache it
         var executedContext = await next();

         // after the controller
         // if value not in cache, save it to cache
         if (executedContext.Result is OkObjectResult okObjectResult)
         {
            await cacheService.CacheResponseAsync(cacheKey, okObjectResult.Value, TimeSpan.FromSeconds(_timeToLiveSeconds));
         }
      }

      private static string GenerateCacheKeyFromRequest(HttpRequest request)
      {
         var keyBuilder = new StringBuilder();

         keyBuilder.Append($"{request.Path}");

         foreach (var (key, value) in request.Query.OrderBy(x => x.Key))
         {
            keyBuilder.Append($"|{key}-{value}"); }

         return keyBuilder.ToString();
      }
   }
}
