using FluentValidation;
using GearShare.Api.DTOs.Items;

namespace GearShare.Api.Validation.Items
{
    public class CreateListingRequestValidator : AbstractValidator<CreateItemRequest>
    {
        public CreateListingRequestValidator()
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
