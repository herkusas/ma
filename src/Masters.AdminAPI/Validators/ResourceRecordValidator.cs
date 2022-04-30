using FluentValidation;
using Masters.AdminAPI.Model;

namespace Masters.AdminAPI.Validators;

internal class ResourceRecordValidator : AbstractValidator<ApiResourceRecord>
{
    public ResourceRecordValidator()
    {
        RuleFor(r => r.Name).NotEmpty().NotNull();
        RuleFor(r => r.Scopes).Must(x => x.Count == x.Distinct(StringComparer.InvariantCultureIgnoreCase).Count())
            .WithMessage("Scopes may not contain duplicates");
    }
};