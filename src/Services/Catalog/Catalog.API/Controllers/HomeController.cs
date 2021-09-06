using Microsoft.AspNetCore.Mvc;

namespace eShopLabs.Services.Catalog.API.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return new RedirectResult("~/swagger");
        }
    }
}
