using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Routing;
using Microsoft.IdentityModel.Protocols;
using Microsoft.Owin;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.Cookies;
using Microsoft.Owin.Security.Notifications;
using Microsoft.Owin.Security.OpenIdConnect;
using Owin;
using Thinktecture.IdentityModel.Client;
using Thinktecture.IdentityServer.AccessTokenValidation;
using Wiki.DiscoveryService.Common;
using Wiki.Security.Handler;
using UserInfoClient = Thinktecture.IdentityModel.Client.UserInfoClient;

[assembly: OwinStartup(typeof(Wiki.DiscoveryService.Startup))]
namespace Wiki.DiscoveryService
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            var config = new HttpConfiguration();

            app.UseIdentityServerBearerTokenAuthentication(new IdentityServerBearerTokenAuthenticationOptions
            {
                Authority = Constants.IdentityServerUrl,
                RequiredScopes = new[] { "service" },
                EnableValidationResultCache = true,
            });

           

            app.AddWikiAuthentication(new WikiAuthenticationOptions
            {
                AccesServerUrl = Constants.IdentityServerUrl,
                AuthenticationType = CookieAuthenticationDefaults.AuthenticationType,
                CookieName = "rik_auth",
                ClientId = Constants.ClientId,
                ClientSecret = Constants.ClientSecret
            });

            app.UseOpenIdConnectAuthentication(new OpenIdConnectAuthenticationOptions
            {
                ClientId = Constants.ClientId,                     
                Authority = Constants.IdentityServerUrl,
                RedirectUri = Constants.RedirectUri,
                PostLogoutRedirectUri = Constants.RedirectUri,
                ResponseType = "code id_token",             
                Scope = "openid web offline_access",
                UseTokenLifetime = false,
                SignInAsAuthenticationType = "Cookies",
                Notifications = new OpenIdConnectAuthenticationNotifications
                {
                    AuthorizationCodeReceived =  AuthorizationCodeReceived(),
                    RedirectToIdentityProvider = n =>
                    {
                        if (n.ProtocolMessage.RequestType == OpenIdConnectRequestType.LogoutRequest)
                        {
                            var idTokenHint = n.OwinContext.Authentication.User.FindFirst("id_token");

                            if (idTokenHint != null)
                            {
                                n.ProtocolMessage.IdTokenHint = idTokenHint.Value;
                            }
                        }

                        return Task.FromResult(0);
                    }
                }

            });
           
            app.UseWebApi(config);
            RegisterRoutes(RouteTable.Routes,config);

            var receiver = new BroadcastReceiver();
            receiver.Start();
            var monitorWatcher = new MonitorWatcher();
            monitorWatcher.Start();

        }

        public static void RegisterRoutes(RouteCollection routes, HttpConfiguration config)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            routes.MapRoute(
                "Default", // Имя маршрута
                "{controller}/{action}/{id}", // URL-адрес с параметрами
                new { controller = "Main", action = "StartPage", id = UrlParameter.Optional } // Параметры по умолчанию
            );
            config.MapHttpAttributeRoutes();
            config.Routes.MapHttpRoute(
                "DefaultApi",
                "api/{controller}/{id}",
                new { id = RouteParameter.Optional }
                );
        }
        private static Func<AuthorizationCodeReceivedNotification, Task> AuthorizationCodeReceived()
        {
            return async n =>
            {
                var tokenClient = new OAuth2Client(new Uri(Constants.TokenEndpoint),
                    Constants.ClientId,
                    Constants.ClientSecret);
                var tokenResponse = await tokenClient.RequestAuthorizationCodeAsync(
                    n.Code, n.RedirectUri);
                if (tokenResponse.IsError)
                {
                    throw new Exception(tokenResponse.Error);
                }
                // use the access token to retrieve claims from userinfo
                var userInfoClient = new UserInfoClient(
                    new Uri(Constants.UserInfoEndpoint), tokenResponse.AccessToken);                        
                var userInfoResponse = await userInfoClient.GetAsync();

                // create new identity
                var id = new ClaimsIdentity(n.AuthenticationTicket.Identity.AuthenticationType);
                id.AddClaims(userInfoResponse.Claims.Select(x=>new Claim(x.Item1,x.Item2)));
                id.AddClaim(new Claim("access_token", tokenResponse.AccessToken));
                id.AddClaim(new Claim("expires_at", DateTime.Now.AddSeconds(tokenResponse.ExpiresIn).ToLocalTime().ToString()));
                id.AddClaim(new Claim("exp", tokenResponse.ExpiresIn.ToString()));
                id.AddClaim(new Claim("refresh_token", tokenResponse.RefreshToken));

                n.AuthenticationTicket = new AuthenticationTicket(
                    new ClaimsIdentity(id.Claims, n.AuthenticationTicket.Identity.AuthenticationType, "name", "role"),
                    n.AuthenticationTicket.Properties);                     
            };
        }
    }

   



}