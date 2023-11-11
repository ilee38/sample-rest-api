using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Tweetbook.Contracts.V1.Responses;

namespace Tweetbook.Filters
{
   public class ValidationFilter : IAsyncActionFilter
   {
      public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
      {
         // Action executed before the controller

         // Since we're using FluentValidator, we can now automatically validate user input
         // against ASP.NET Core's ModelState.
         if (!context.ModelState.IsValid)
         {
            var errorsInModelState = context.ModelState
               .Where(x => x.Value.Errors.Count > 0)
               .ToDictionary(kv => kv.Key, kv => kv.Value.Errors.Select(x => x.ErrorMessage)).ToArray();

            var errorResponse = new ErrorResponse();

            foreach (var error in errorsInModelState)
            {
               foreach (var subError in error.Value)
               {
                  var errorModel = new ErrorModel
                  {
                     FieldName = error.Key,
                     Message = subError
                  };

                  errorResponse.Errors.Add(errorModel);
               }
            }
            context.Result = new BadRequestObjectResult(errorResponse);
            return;
         }

         await next();

         // Action executed after the controller
      }
   }
}
