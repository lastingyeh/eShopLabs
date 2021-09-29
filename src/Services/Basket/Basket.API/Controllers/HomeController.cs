using Microsoft.AspNetCore.Mvc;

namespace eShopLabs.Services.Basket.API.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index() => new RedirectResult("~/swagger");
    }
}