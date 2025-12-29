using FastEndpoints;
using FluentValidation;

namespace AspireAppTemplate.ApiService.Features.CustomJobs.Create;

public class Validator : Validator<Request>
{
    public Validator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("任務名稱不可為空")
            .MaximumLength(100).WithMessage("任務名稱不可超過 100 字元");

        RuleFor(x => x.Url)
            .NotEmpty().WithMessage("URL 不可為空")
            .Must(BeValidUrl).WithMessage("URL 格式不正確");

        RuleFor(x => x.CronExpression)
            .NotEmpty().When(x => x.Type == Data.Entities.JobType.Recurring)
            .WithMessage("週期性任務必須提供 Cron 表達式");

        RuleFor(x => x.ScheduledAt)
            .NotNull().When(x => x.Type == Data.Entities.JobType.OneTime)
            .WithMessage("一次性任務必須提供執行時間")
            .GreaterThan(DateTime.UtcNow).When(x => x.Type == Data.Entities.JobType.OneTime)
            .WithMessage("執行時間必須在未來");

        RuleFor(x => x.Headers)
            .Must(BeValidJson).When(x => !string.IsNullOrEmpty(x.Headers))
            .WithMessage("Headers 必須是有效的 JSON 格式");

        RuleFor(x => x.Body)
            .Must(BeValidJson).When(x => !string.IsNullOrEmpty(x.Body))
            .WithMessage("Body 必須是有效的 JSON 格式");
    }

    private bool BeValidUrl(string url)
    {
        return Uri.TryCreate(url, UriKind.Absolute, out var uriResult)
               && (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);
    }

    private bool BeValidJson(string? json)
    {
        if (string.IsNullOrWhiteSpace(json)) return true;
        
        try
        {
            System.Text.Json.JsonDocument.Parse(json);
            return true;
        }
        catch
        {
            return false;
        }
    }
}
