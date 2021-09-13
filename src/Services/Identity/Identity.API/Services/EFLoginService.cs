using System.Threading.Tasks;
using eShopLabs.Services.Identity.API.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;

namespace eShopLabs.Services.Identity.API.Services
{
    public class EFLoginService : ILoginService<ApplicationUser>
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        public EFLoginService(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager)
        {
            _signInManager = signInManager;
            _userManager = userManager;
        }
        public Task<ApplicationUser> FindByUsername(string user)
        {
            return _userManager.FindByEmailAsync(user);
        }

        public Task SignIn(ApplicationUser user)
        {
            return _signInManager.SignInAsync(user, true);
        }

        public Task SignInAsync(ApplicationUser user, AuthenticationProperties properties, string authenticationMethod = null)
        {
            return _signInManager.SignInAsync(user, properties, authenticationMethod);
        }

        public Task<bool> ValidateCredentials(ApplicationUser user, string password)
        {
            return _userManager.CheckPasswordAsync(user, password);
        }
    }
}