namespace Tweetbook.Services
{
   public interface IResponseCacheService
   {
      Task CacheResponseAsync(string cacheKey, object response, TimeSpan timeToLive);

      Task<string> GetCacheResponseAsync(string cacheKey);
   }
}