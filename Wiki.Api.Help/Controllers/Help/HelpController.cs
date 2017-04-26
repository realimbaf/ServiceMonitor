using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;
using Newtonsoft.Json;
using RazorEngine;
using RazorEngine.Templating;
using Wiki.Api.Help.Controllers.Help.Model;
using Wiki.Api.Help.Extensions;
using Wiki.Api.Help.Model;

namespace Wiki.Api.Help.Controllers.Help
{
    /// <summary>
    /// Generate Api documentation
    /// </summary>
    [RoutePrefix("help")]
    [ApiExplorerSettings(IgnoreApi = true)]
    [AllowAnonymous]
    public class HelpController : ApiController
    {
        /// <summary>
        /// Main Api documentation page
        /// </summary>
        /// <returns></returns>
        [Route("")]
        [HttpGet]
        public HttpResponseMessage Index()
        {
            var model = this.Configuration.Services.GetApiExplorer().ApiDescriptions;
            var viewBag = new DynamicViewBag();
            viewBag.AddValue("DocumentationProvider", this.Configuration.Services.GetDocumentationProvider());
            string result = Engine.Razor.RunCompile("Help.Index.cshtml", model.GetType(), model, viewBag);


            return new HttpResponseMessage() { Content = new StringContent(result, System.Text.Encoding.UTF8, "text/html") };
        }

        /// <summary>
        /// Documentation for Api method
        /// </summary>
        /// <param name="apiId">API method key</param>
        /// <returns></returns>
        [Route("apidoc/{apiId}")]
        [HttpGet]
        public HttpResponseMessage ApiDocumentation(string apiId)
        {
            if (!string.IsNullOrEmpty(apiId))
            {
                HelpPageApiModel apiModel = this.Configuration.GetHelpPageApiModel(apiId);
                if (apiModel != null)
                {
                    string result = Engine.Razor.RunCompile("Help.ApiDoc.cshtml", apiModel.GetType(), apiModel);
                    return new HttpResponseMessage() { Content = new StringContent(result, System.Text.Encoding.UTF8, "text/html") };
                }
            }
            throw new HttpResponseException(HttpStatusCode.NotFound);
        }

        [Route("api")]
        [HttpGet]
        [ResponseType(typeof(List<HelpPageApiModel>))]
        public async Task<HttpResponseMessage> GeterateApiDoc()
        {
            
            List<HelpPageApiModel> model = Configuration.GenerateApiModel();
            var jsonSerializerSettings = new JsonSerializerSettings()
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore
            };
            var str = Newtonsoft.Json.JsonConvert.SerializeObject(model,jsonSerializerSettings);
            return new HttpResponseMessage() { Content = new StringContent(str, System.Text.Encoding.UTF8, "text/html") }; ;
        }

        /// <summary>
        /// Documentation for model
        /// </summary>
        /// <param name="modelName">Model name</param>
        /// <returns></returns>
        [Route("model/{modelName}")]
        [HttpGet]
        public HttpResponseMessage ResourceModel(string modelName)
        {
            if (!String.IsNullOrEmpty(modelName))
            {
                ModelDescriptionGenerator modelDescriptionGenerator = this.Configuration.GetModelDescriptionGenerator();
                ModelDescription modelDescription;
                if (modelDescriptionGenerator.GeneratedModels.TryGetValue(modelName, out modelDescription))
                {
                    string result = Engine.Razor.RunCompile("Help.ResourceModel.cshtml", modelDescription.GetType(), modelDescription);
                    return new HttpResponseMessage() { Content = new StringContent(result, System.Text.Encoding.UTF8, "text/html") };
                }
            }

            throw new HttpResponseException(HttpStatusCode.NotFound);
        }

        [HttpPost]
        [Route("aaa")]
        public HttpResponseMessage Test()
        {
            return null;
        }

    }


}