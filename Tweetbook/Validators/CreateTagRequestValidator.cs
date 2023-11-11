using FluentValidation;
using Tweetbook.Contracts.V1.Requests;

namespace Tweetbook.Validators
{
   public class CreateTagRequestValidator : AbstractValidator<CreateTagRequest>
   {
      public CreateTagRequestValidator()
      {
         RuleFor(x => x.TagName)
            .NotNull()
            .NotEmpty()
            .Matches("^[a-zA-Z0-9 ]*$");
      }
   }
}