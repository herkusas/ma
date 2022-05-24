using System.Text.RegularExpressions;
using FluentValidation;
using Ma.AdminAPI.Model;

namespace Ma.AdminAPI.Validators
{
    // ReSharper disable once UnusedType.Global
    public class RobotRecordValidator: AbstractValidator<RobotRecord>
    {
        public RobotRecordValidator()
        {
            RuleFor(r => r.RobotId).NotEmpty().NotNull();
            RuleFor(r => r.Name).NotEmpty().NotNull();
            RuleFor(r => r.Scopes).Must(x => x.Count == x.Distinct(StringComparer.InvariantCultureIgnoreCase).Count())
                .WithMessage("Scopes may not contain duplicates");
            RuleFor(r => r.Thumbprints).Must(x => x.Count == x.Distinct(StringComparer.InvariantCultureIgnoreCase).Count())
                .WithMessage("Secrets may not contain duplicates")
                .DependentRules(() =>
                {
                    RuleFor(r => r.Thumbprints).Must(r => r.All(s => Regex.IsMatch(s, "^[a-fA-F0-9]{40}$")))
                        .WithMessage("Check if all thumbprints are valid (only SHA1 accepted)");
                });
        }
    }
}