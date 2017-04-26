using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Reflection;
using System.Security.Claims;
using System.Web.Http;
using System.Web.Http.Description;
using System.Web.Http.Results;
using Microsoft.Owin.Security;

namespace Wiki.ServiceMonitor.Controllers
{
    [RoutePrefix("config")]
    public class ConfigController : ApiController
    {

        [Route("~/status")]
        [HttpGet]
        public HttpResponseMessage InfoPage()
        {
            //if (User==null||!User.Identity.IsAuthenticated)
            //{
            //    var response = Request.CreateResponse(HttpStatusCode.Moved);
            //    response.Headers.Location = new Uri(Url.Content("~/login"));
            //    return response;
            //}
            var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("Wiki.ServiceMonitor.Content.Pages.Config.html");
            var infos = Assembly.GetExecutingAssembly().GetManifestResourceNames();
            if (stream == null)
                throw new KeyNotFoundException(string.Join("\n", infos));
            using (StreamReader reader = new StreamReader(stream))
            {
                var page = reader.ReadToEnd();
                return new HttpResponseMessage() { Content = new StringContent(page, System.Text.Encoding.UTF8, "text/html") };
            }
        }
        [Route("~/info/{id}")]
        [HttpGet]
        public HttpResponseMessage ConcreteServicePage(string id)
        {
            //if (User==null||!User.Identity.IsAuthenticated)
            //{
            //    var response = Request.CreateResponse(HttpStatusCode.Moved);
            //    response.Headers.Location = new Uri(Url.Content("~/login"));
            //    return response;
            //}
            var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("Wiki.ServiceMonitor.Content.Pages.Service.html");
            var infos = Assembly.GetExecutingAssembly().GetManifestResourceNames();
            if (stream == null)
                throw new KeyNotFoundException(string.Join("\n", infos));
            using (StreamReader reader = new StreamReader(stream))
            {
                var page = reader.ReadToEnd();
                return new HttpResponseMessage() { Content = new StringContent(page, System.Text.Encoding.UTF8, "text/html") };
            }
        }


        [Route("~/login")]
        [HttpGet]
        [AllowAnonymous]
        [ApiExplorerSettings(IgnoreApi = true)]
        public RedirectResult Login()
        {
            var claimsIdentity = new ClaimsIdentity("Cookies", "name", "role");
            claimsIdentity.AddClaim(new Claim("name","test"));


            var principial = new ClaimsPrincipal(claimsIdentity);
            var tiket = new AuthenticationTicket(principial.Identity as ClaimsIdentity,
                new AuthenticationProperties { IsPersistent = true });

            Request.GetOwinContext().Authentication.SignIn(tiket.Properties, tiket.Identity);


            return Redirect(Url.Content("~/status"));
        }

    }
}