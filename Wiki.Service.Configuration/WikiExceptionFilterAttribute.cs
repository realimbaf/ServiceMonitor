using System;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Filters;
using CarParts.Common.Log;

namespace Wiki.Service.Configuration
{
    public class WikiExceptionFilterAttribute: ExceptionFilterAttribute
    {
        FileLogger _logger=new FileLogger("web_error");
        public override void OnException(HttpActionExecutedContext context)
        {
            this._logger.WriteError("Web error.Url:"+context.Request.RequestUri.AbsoluteUri,context.Exception);
            context.Response = context.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, new HttpError(context.Exception, true));

        }
    }
}