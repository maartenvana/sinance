using Microsoft.AspNetCore.Mvc;
using Sinance.Common.Configuration;

namespace Sinance.Web.ViewComponents
{
    public class VersionNumberViewComponent : ViewComponent
    {
        private readonly AppSettings _appSettings;

        public VersionNumberViewComponent(AppSettings appSettings)
        {
            _appSettings = appSettings;
        }

        public IViewComponentResult Invoke()
        {
            return View(_appSettings.SinanceVersion);
        }
    }
}
