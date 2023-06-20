using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Sinance.Common.Configuration;

namespace Sinance.Web.ViewComponents;

public class VersionNumberViewComponent : ViewComponent
{
    private readonly AppSettings _appSettings;

    public VersionNumberViewComponent( AppSettings appSettings)
    {
        _appSettings = appSettings;
    }

    public IViewComponentResult Invoke()
    {
        var version = _appSettings.SinanceVersion;

        return View(model: version);
    }
}