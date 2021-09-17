using System.Threading.Tasks;
using eShopLabs.Services.Identity.API.Models;
using eShopLabs.Services.Identity.API.Services;
using IdentityServer4.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace eShopLabs.Services.Identity.API.Controllers
{
    public class HomeController : Controller
    {
        private readonly IIdentityServerInteractionService _interaction;
        private readonly IRedirectService _redirectService;
        private readonly IOptionsSnapshot<AppSettings> _settings;
        public HomeController(
            IIdentityServerInteractionService interaction,
            IOptionsSnapshot<AppSettings> settings,
            IRedirectService redirectService)
        {
            _settings = settings;
            _redirectService = redirectService;
            _interaction = interaction;
        }

        public IActionResult Index(string returnUrl)
        {
            return View();
        }

        public IActionResult ReturnToOriginalApplication(string returnUrl)
        {
            if (returnUrl != null)
            {
                return Redirect(_redirectService.ExtractRedirectUriFromReturnUrl(returnUrl));
            }

            return RedirectToAction("Index", "Home");
        }

        public async Task<IActionResult> Error(string errorId)
        {
            var vm = new ErrorViewModel();
            var message = await _interaction.GetErrorContextAsync(errorId);

            if (message != null)
            {
                vm.Error = message;
            }

            return View("Error", vm);
        }
    }
}