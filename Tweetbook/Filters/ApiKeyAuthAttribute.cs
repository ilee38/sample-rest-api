using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Tweetbook.Filters
{
   /// <summary>
   /// Middleware class to add ApiKey auth to our REST API.
   /// By specifying the "AttributeUssage" below, we can use this middleware in a controller to require ApiKey auth on a
   /// per controller basis or a per request method basis (we need to add the appropriate tags in the controller class).
   /// </summary>
   [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
   public class ApiKeyAuthAttribute : Attribute, IAsyncActionFilter
   {
      public const string ApiKeyHeaderName = "ApiKey";
      public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
      {
         // Action executed before the controller
         if (!context.HttpContext.Request.Headers.TryGetValue(ApiKeyHeaderName, out var potentialApiKey))
         {
            context.Result = new UnauthorizedResult();
            // Here we return so that the flow does not go to the controller
            return;
         }

         // Here we need to get the actual api key from our appsettings.json. so we invoke the IConfiguration service
         var configuration = context.HttpContext.RequestServices.GetRequiredService<IConfiguration>();
         var apiKey = configuration.GetValue<string>("ApiKey");

         // Then we compare if both api keys match
         if (!apiKey.Equals(potentialApiKey))
         {
            context.Result = new UnauthorizedResult();
            // Here we return so that the flow does not go to the controller
            return;
         }

         await next();

         // Action executed after the controller
      }
   }
}
