using FastEndpoints;
using FluentValidation;

namespace AspireAppTemplate.ApiService.Features.Jobs.LogCleanup.Create;

public class Validator : Validator<Request>
{
    public Validator()
    {
        RuleFor(x => x.RetentionDays)
            .GreaterThan(0)
            .WithMessage("保留天數必須大於 0");
        
        RuleFor(x => x.ScheduleDays)
            .GreaterThan(0)
            .WithMessage("排程天數必須大於 0");
    }
}
