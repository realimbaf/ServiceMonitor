using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Net;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Filters;
using Microsoft.Owin;
using Microsoft.Owin.FileSystems;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.Cookies;
using Microsoft.Owin.Security.OpenIdConnect;
using Microsoft.Owin.StaticFiles;
using Newtonsoft.Json.Serialization;
using Owin;
using Thinktecture.IdentityServer.AccessTokenValidation;
using Wiki.Api.Help;
using Wiki.Service.Configuration;

namespace Wiki.ServiceMonitor
{
    public class Startup
    {

        public static string GetIdentityServerUrl()
        {
            return ConfigurationManager.AppSettings["IdentityServerUrl"];
        }

        public static string SelfUrl
        {
            get { return MonitorSrv.MonitorUrl.Replace("+", Dns.GetHostName()); }
        }

        public void Configuration(IAppBuilder app)
        {
            // Configure Web API for self-host. 
            var config = new HttpConfiguration();

            HelpExtension.AddCss("<link type='text/css' href='/Content/HelpPage.css' rel='stylesheet' />");

            app.AddApiHelp(config);
            ConfigAuthentication(app);


            config.Formatters.JsonFormatter.SerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();

            config.MapHttpAttributeRoutes();

            app.UseFileServer(new FileServerOptions
            {
                RequestPath = new PathString("/fonts"),
                FileSystem = new EmbeddedResourceFileSystem(typeof(Startup).Assembly, "Wiki.ServiceMonitor.fonts")
            });

            app.UseFileServer(new FileServerOptions
            {
                RequestPath = new PathString("/Content"),
                FileSystem = new EmbeddedResourceFileSystem(typeof(Startup).Assembly, "Wiki.ServiceMonitor.Content")
            });

            app.UseFileServer(new FileServerOptions
            {
                RequestPath = new PathString("/Scripts"),
                FileSystem = new EmbeddedResourceFileSystem(typeof(Startup).Assembly, "Wiki.ServiceMonitor.Scripts")
            });

            config.Routes.MapHttpRoute(
                name: "DefaultApi",
                routeTemplate: "api/{controller}/{id}",
                defaults: new { id = RouteParameter.Optional }
                );
#if DEBUG
            app.Use<HeaderMiddleware>();
#endif
            app.UseWebApi(config);


            config.Filters.Add(new AuthorizeAttribute());
            config.Filters.Add(new WikiExceptionFilterAttribute());


            config.EnsureInitialized();



        }



        public class HeaderMiddleware
        {
            private Func<IDictionary<string, object>, Task> _next;

            public HeaderMiddleware(Func<IDictionary<string, object>, Task> next)
            {
                if (next == null)
                    throw new ArgumentNullException("next");
                this._next = next;

            }



            public async Task Invoke(IDictionary<string, object> environment)
            {
                IOwinContext context = (IOwinContext)new OwinContext(environment);

                context.Response.OnSendingHeaders(x =>
                {
                    context.Response.Headers.Add("Access-Control-Allow-Origin", new[] { "*" });
                }, null);

                await _next.Invoke(environment);
            }
        }


        private void ConfigAuthentication(IAppBuilder app)
        {
            app.UseIdentityServerBearerTokenAuthentication(new IdentityServerBearerTokenAuthenticationOptions
            {
                Authority = GetIdentityServerUrl(),
                RequiredScopes = new[] { "service" },
                EnableValidationResultCache = true,
            });


            app.UseCookieAuthentication(new CookieAuthenticationOptions
            {
                AuthenticationType = "Cookies",
                CookieName = "_authMonitor_",
                //LoginPath = new PathString("/login")
            });



        }
    }
}