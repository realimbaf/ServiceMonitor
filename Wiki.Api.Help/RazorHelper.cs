using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using RazorEngine;
using RazorEngine.Configuration;
using RazorEngine.Templating;
using RazorEngine.Text;

namespace Wiki.Api.Help
{
    public class RazorHelper
    {


        public static void InitRazor()
        {
            var config = new TemplateServiceConfiguration();

            config.Language = Language.CSharp; // VB.NET as template language.
            config.EncodedStringFactory = new RawStringFactory(); // Raw string encoding.
            config.EncodedStringFactory = new HtmlEncodedStringFactory(); // Html encoding.
            config.DisableTempFileLocking = true;
            config.CachingProvider = new DefaultCachingProvider(t =>
            {
                try
                {
                    Directory.Delete(t, true);
                }
                catch(Exception e)
                {
                }
            });
            string viewPathTemplate = "Wiki.Api.Help.Views.{0}";


            config.TemplateManager = new DelegateTemplateManager(name =>
            {
                string resourcePath = string.Format(viewPathTemplate, name);
                var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(resourcePath);
                if(stream==null)
                    throw new KeyNotFoundException(string.Format("View {0} not found", resourcePath));
                using (StreamReader reader = new StreamReader(stream))
                {
                    return reader.ReadToEnd();
                }
            });

                        
            Engine.Razor = RazorEngineService.Create(config); 
        }


    }

}