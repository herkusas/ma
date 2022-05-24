using FluentValidation;
using Ma.Model;

namespace Ma.AdminAPI.Validators;

// ReSharper disable once UnusedType.Global
internal class ResourceRecordValidator : AbstractValidator<ApiResourceRecord>
{
    public ResourceRecordValidator()
    {
        RuleFor(r => r.Name).NotEmpty().NotNull();
        RuleFor(r => r.Scopes).Must(x => x.Count == x.Distinct(StringComparer.InvariantCultureIgnoreCase).Count())
            .WithMessage("Scopes may not contain duplicates");
    }
}
