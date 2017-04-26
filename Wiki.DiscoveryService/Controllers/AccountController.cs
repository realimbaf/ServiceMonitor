using System.Net.Http;
using System.Web.Http;
using Microsoft.Owin.Security;
using Wiki.DiscoveryService.Common;

namespace Wiki.DiscoveryService.Controllers
{
    [RoutePrefix("api/accounts")]
    public class AccountController : ApiController
    {
        [Route("logout")]
        [HttpGet]
        [AllowAnonymous]
        public IHttpActionResult Logout()
        {
            if (Request.GetOwinContext().Authentication.User.Identity.IsAuthenticated)
            {
                var properties = new AuthenticationProperties
                {
                    RedirectUri = Constants.RedirectUri
                };

                Request.GetOwinContext().Authentication.SignOut(properties);
            }
            return Redirect("/");
        }
    }
}