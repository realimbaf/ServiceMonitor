using System;
using System.Collections.Generic;
using System.Web.Http;
using System.Web.Http.Description;
using Owin;
using Microsoft.Owin.StaticFiles;
using Microsoft.Owin.FileSystems;
using Microsoft.Owin;
using Thinktecture.IdentityServer.AccessTokenValidation;
using Wiki.Api.Help;
using Wiki.Service.Common.Clients;
using Wiki.Service.Configuration;

namespace Wiki.ServiceHost
{
    public class Startup
    {

        private static List<IServiceConfig> _configs=new List<IServiceConfig>();


        public void Configuration(IAppBuilder app)
        {
            var config = new HttpConfiguration();

            app.AddApiHelp(config);


            ConfigAuthentication(app);

            app.Use<ShutDownMiddleware>();

            config.MapHttpAttributeRoutes();

            app.UseFileServer(new FileServerOptions
            {
                RequestPath = new PathString("/Content"),
                FileSystem = new EmbeddedResourceFileSystem(typeof(Startup).Assembly, "Wiki.ServiceHost.Content")
            });

            app.UseFileServer(new FileServerOptions
            {
                RequestPath = new PathString("/Scripts"),
                FileSystem = new EmbeddedResourceFileSystem(typeof(Startup).Assembly, "Wiki.ServiceHost.Scripts")
            });

            config.Routes.MapHttpRoute(
                name: "DefaultApi",
                routeTemplate: "api/{controller}/{id}",
                defaults: new { id = RouteParameter.Optional }
                );

            app.UseWebApi(config);


            this.Init(app, config);

            config.Filters.Add(new AuthorizeAttribute());
            config.Filters.Add(new WikiExceptionFilterAttribute());


            config.EnsureInitialized();                   
        }

        private void ConfigAuthentication(IAppBuilder app)
        {
            app.UseIdentityServerBearerTokenAuthentication(new IdentityServerBearerTokenAuthenticationOptions
            {
                Authority = GetIdentityServerUrl(),
                RequiredScopes = new[] { "service" },
                EnableValidationResultCache = true
            });
        }


        private string GetIdentityServerUrl()
        {
            var url = ConfigurationContainer.Configuration["IdentityServerUrl"];
            if (string.IsNullOrEmpty(url))
            {
                var cl = new ServiceMonitorClient();
                url = cl.IdentityServerUrl;
                ConfigurationContainer.Configuration["IdentityServerUrl"] = url;
                ConfigurationContainer.Configuration.Save();
            }
            return url;
        }

        private void Init(IAppBuilder app, HttpConfiguration config)
        {
            foreach (var cfg in _configs)
            {
                try
                {
                    cfg.Init(app, config);
                }
                catch (Exception e)
                {
                    MainSrv.Logger.WriteError("Error init configuration.",e);
                }
            }
        }


        public static void LoadConfig(ICollection<IServiceConfig> cfg)
        {
            _configs.Clear();
            _configs.AddRange(cfg);
        }
    }
}