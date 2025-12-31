using AspireAppTemplate.Shared.Resources;
using Microsoft.Extensions.Localization;
using MudBlazor;

namespace AspireAppTemplate.Web.Infrastructure.Localization;

public class AppMudLocalizer : MudLocalizer
{
    private readonly IStringLocalizer<SharedResource> _localizer;

    public AppMudLocalizer(IStringLocalizer<SharedResource> localizer)
    {
        _localizer = localizer;
    }

    public override LocalizedString this[string key] => _localizer[key];
}
