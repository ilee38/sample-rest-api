
using System.Text.Json;
using Microsoft.Extensions.Caching.Distributed;

namespace Tweetbook.Services
{
   public class ResponseCacheService : IResponseCacheService
   {
      private readonly IDistributedCache _distributedCache;

      public ResponseCacheService(IDistributedCache distributedCache)
      {
         _distributedCache = distributedCache;
      }

      public async Task CacheResponseAsync(string cacheKey, object response, TimeSpan timeToLive)
      {
         if (response == null)
         {
            return;
         }

         var serializedResponse = JsonSerializer.Serialize(response);

         await _distributedCache.SetStringAsync(cacheKey, serializedResponse, new DistributedCacheEntryOptions
         {
            AbsoluteExpirationRelativeToNow = timeToLive
         });
      }

      public async Task<string> GetCacheResponseAsync(string cacheKey)
      {
         var cachedResponse = await _distributedCache.GetStringAsync(cacheKey);

         return string.IsNullOrEmpty(cachedResponse) ? null : cachedResponse;
      }
   }
}
