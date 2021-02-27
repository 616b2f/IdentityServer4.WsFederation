using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using IdentityModel;
using IdentityServer4.Services;
using IdentityServer4.Test;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;

namespace IdentityServer4.WsFederation.Tests
{
    public class FakeAccountController : Controller
    {
        private readonly TestUserStore _userStore;
        private readonly IIdentityServerInteractionService _interaction;

        public FakeAccountController(
            TestUserStore userStore,
            IIdentityServerInteractionService interaction)
        {
            _userStore = userStore;
            _interaction = interaction;
        }

        [HttpGet]
        [Route("account/login")]
        public async Task<IActionResult> Login(string subjectId, string returnUrl)
        {
            var user = _userStore.FindBySubjectId(subjectId);
            var identity = new ClaimsIdentity(user.Claims.ToList(), "Fake IdP", JwtClaimTypes.Name, JwtClaimTypes.Role);
            var principal = new ClaimsPrincipal(identity);
            await HttpContext.SignInAsync("idsrv", principal);
            if(Url.IsLocalUrl(returnUrl))
                return Redirect(returnUrl);
            return Ok();
        }
        
        [HttpGet]
        [Route("account/logout")]
        public async Task<IActionResult> Logout(string logoutId)
        {
            // // var user = _userStore.FindBySubjectId(subjectId);
            // // var identity = new ClaimsIdentity(user.Claims.ToList(), "Fake IdP", JwtClaimTypes.Name, JwtClaimTypes.Role);
            // // var principal = new ClaimsPrincipal(identity);
            var logout = await _interaction.GetLogoutContextAsync(logoutId);
            await HttpContext.SignOutAsync();
            if(string.IsNullOrEmpty(logout?.PostLogoutRedirectUri) ||
               Url.IsLocalUrl(logout.PostLogoutRedirectUri))
            {
                return Redirect(logout.PostLogoutRedirectUri);
            }
            return Ok();
        }
    }
}