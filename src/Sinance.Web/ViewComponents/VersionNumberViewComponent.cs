using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Sinance.Common.Configuration;

namespace Sinance.Web.ViewComponents
{
    public class VersionNumberViewComponent : ViewComponent
    {
        private readonly AppSettings _appSettings;
        private readonly IConfigurationRoot _configurationRoot;

        public VersionNumberViewComponent(
            AppSettings appSettings,
            IConfigurationRoot configurationRoot)
        {
            _appSettings = appSettings;
            _configurationRoot = configurationRoot;
        }

        public IViewComponentResult Invoke()
        {
            var version = _appSettings.SinanceVersion;
            var sourceBranch = _configurationRoot["SOURCE_BRANCH"];

            if (!string.IsNullOrWhiteSpace(sourceBranch) &&
                sourceBranch != "master")
            {
                version += $"-{sourceBranch}";
            }

            return View(model: version);
        }
    }
}