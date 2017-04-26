using System.Linq;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Description;
using Microsoft.Owin;
using Microsoft.Owin.FileSystems;
using Microsoft.Owin.StaticFiles;
using Owin;
using Wiki.Api.Help.Controllers.Help;
using Wiki.Api.Help.Extensions;
using Wiki.Core.Api;

namespace Wiki.Api.Help
{
    public static class HelpExtension
    {
        private static string _addCss;

        public static void AddApiHelp(this IAppBuilder app, HttpConfiguration config)
        {
            RazorHelper.InitRazor();

            app.UseFileServer(new FileServerOptions
            {
                RequestPath = new PathString("/help/content"),
                FileSystem = new EmbeddedResourceFileSystem(typeof(HelpExtension).Assembly, "Wiki.Api.Help.Content")
            });
            config.Services.Replace(typeof(IDocumentationProvider), new WikiDocumentGenerator());

        }

        public static string GetIndex(this HttpControllerDescriptor controllerDescriptor)
        {
            if (controllerDescriptor == null)
                return string.Empty;
            var idx = int.MaxValue;
            var attr = controllerDescriptor.GetCustomAttributes<ControllerOrderAttribute>().ToList();
            if (attr.Count > 0)
                idx = attr[0].Order;

            return idx.ToString("00000000000") + controllerDescriptor.ControllerName;
        }

        public static bool IsShowOpenUrl(this HttpControllerDescriptor controllerDescriptor)
        {
            if (controllerDescriptor == null)
                return true;
            var attr = controllerDescriptor.GetCustomAttributes<HelpShowOpenUrlAttribute>().ToList();
            return attr.Count <= 0 || attr[0].IsShow;
        }
        public static void AddCss(string cssString)
        {
            _addCss = cssString;
        }

        public static string GetCss()
        {
            return _addCss ?? "";
        }
    }
                
}