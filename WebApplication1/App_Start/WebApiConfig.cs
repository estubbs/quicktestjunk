using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Web.Http.ExceptionHandling;
using System.Web.Http.Results;

namespace WebApplication1
{
    public class ExceptionH : System.Web.Http.ExceptionHandling.ExceptionHandler
    {
        public override void Handle(ExceptionHandlerContext context)
        {
            HttpContext.Current.Response.TrySkipIisCustomErrors = true;
            context.Result = null;
            context.Result = new System.Web.Http.Results.InternalServerErrorResult(context.Request);
            
        }
        

    }
    public static class WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {
            
            config.Services.Replace(typeof(IExceptionHandler), new ExceptionH());
            

            // Web API configuration and services

            // Web API routes
            config.MapHttpAttributeRoutes();

            config.Routes.MapHttpRoute(
                name: "DefaultApi",
                routeTemplate: "api/{controller}/{id}",
                defaults: new { id = RouteParameter.Optional }
            );
            
            
            var thing = config.Services.GetExceptionHandler();
            

        }
    }
}
