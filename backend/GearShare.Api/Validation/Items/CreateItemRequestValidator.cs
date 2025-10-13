using FluentValidation;
using GearShare.Api.DTOs.Items;

namespace GearShare.Api.Validation.Items
{
    public class CreateItemRequestValidator : AbstractValidator<CreateItemRequest>
    {
        public CreateItemRequestValidator()
        {
            RuleFor(x => x.Title)
                .NotEmpty()
                .MaximumLength(120);

            RuleFor(x => x.Description)
                .MaximumLength(2000);

            RuleFor(x => x.Condition)
                .NotEmpty();
        }
    }
}
