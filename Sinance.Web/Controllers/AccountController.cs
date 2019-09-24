using Sinance.Domain.Entities;
using Sinance.Web.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using IAuthenticationService = Sinance.Business.Services.Authentication.IAuthenticationService;

namespace Sinance.Controllers
{
    /// <summary>
    /// Account controller to handle all account related actions
    /// </summary>
    [Authorize]
    public class AccountController : Controller
    {
        private readonly IAuthenticationService _authenticationService;
        private readonly IHttpContextAccessor _httpContextAccessor;

        /// <summary>
        /// Default constructor
        /// </summary>
        public AccountController(
            IAuthenticationService authenticationService,
            IHttpContextAccessor httpContextAccessor)
        {
            _authenticationService = authenticationService;
            _httpContextAccessor = httpContextAccessor;
        }

        /// <summary>
        /// Failure of the external login
        /// </summary>
        /// <returns>View for external login failure</returns>
        [AllowAnonymous]
        public ActionResult ExternalLoginFailure()
        {
            return View();
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult Login()
        {
            return View();
        }

        /// <summary>
        /// Login acction for returning the login view
        /// </summary>
        /// <param name="returnUrl">Return url to return to after logging in</param>
        /// <returns>Login view</returns>
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Login(LoginViewModel loginViewModel, string returnUrl)
        {
            var user = _authenticationService.SignIn(loginViewModel.UserName, loginViewModel.Password);
            if (user == null)
            {
                ModelState.AddModelError("", "User not found");
                return View();
            }

            await SignInSinanceUser(user);

            return RedirectToLocal(returnUrl);
        }

        /// <summary>
        /// Logs the current user out
        /// </summary>
        /// <returns>Return to homepage</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> LogOff()
        {
            await HttpContext.SignOutAsync();

            return RedirectToAction("Index", "Home");
        }

        /// <summary>
        /// Provides the register page
        /// </summary>
        /// <returns>The register page</returns>
        [AllowAnonymous]
        public ActionResult Register()
        {
            return View();
        }

        /// <summary>
        /// Handles the event of registering
        /// </summary>
        /// <param name="model">Model to use for registering</param>
        /// <returns>Result of register</returns>
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Register(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                // Activate the user immediately
                var user = await _authenticationService.CreateUser(model.UserName, model.Password);

                await SignInSinanceUser(user);

                return RedirectToAction("Index", "Home");
            }

            return View(model);
        }

        /// <summary>
        /// Redirects the return url to a local url
        /// </summary>
        /// <param name="returnUrl">Url to redirect</param>
        /// <returns>Redirect for the return url</returns>
        private ActionResult RedirectToLocal(string returnUrl)
        {
            return Url.IsLocalUrl(returnUrl) ? Redirect(returnUrl) : (ActionResult)RedirectToAction("Index", "Home");
        }

        private async Task SignInSinanceUser(SinanceUser user)
        {
            var identity = new ClaimsIdentity(CookieAuthenticationDefaults.AuthenticationScheme);
            identity.AddClaim(new Claim(type: ClaimTypes.Name, value: user.Username));
            identity.AddClaim(new Claim(type: "UserId", value: user.Id.ToString()));

            var principal = new ClaimsPrincipal(identity);
            await _httpContextAccessor.HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);
        }
    }
}