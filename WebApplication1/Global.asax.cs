using Serilog;
using Serilog.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using Serilog.Events;
using System.Collections.Specialized;

namespace WebApplication1
{

    public class RequeUriEnricher : ILogEventEnricher
    {
        public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
        {
            var context = HttpContext.Current;
            if (context == null)
                return;

            var request = context.Request;
            var response = context.Response;

            if (request != null)
            {
                _EnrichRequest(request, logEvent, propertyFactory);
            }
            if (response != null)
            {
                _EnrichResponse(response, logEvent, propertyFactory);
            }
        }
        void _EnrichResponse(HttpResponse httpResponse, LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
        {
            var headers = transformHeaders(httpResponse.Headers);
            logEvent.AddOrUpdateProperty(propertyFactory.CreateProperty("ResponseHeaders", headers));
            logEvent.AddOrUpdateProperty(propertyFactory.CreateProperty("ResponseStatusCode", httpResponse.StatusCode));
        }
        private void _EnrichRequest(HttpRequest request, LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
        {
            var method = request.HttpMethod;
            var isSecure = request.IsSecureConnection;
            var ipAddress = request.UserHostAddress;
            var uri = request.Url;

            var headers = request.Headers;
            Dictionary<string, string[]> allHeaders = transformHeaders(headers);

            logEvent.AddOrUpdateProperty(propertyFactory.CreateProperty("HttpMethod", method));
            logEvent.AddOrUpdateProperty(propertyFactory.CreateProperty("IsHttps", isSecure));
            logEvent.AddOrUpdateProperty(propertyFactory.CreateProperty("ClientIpAddress", ipAddress));
            logEvent.AddOrUpdateProperty(propertyFactory.CreateProperty("RequestHeaders", allHeaders));
            logEvent.AddOrUpdateProperty(propertyFactory.CreateProperty("RequestUri", uri));

        }

        private static Dictionary<string, string[]> transformHeaders(NameValueCollection headers)
        {
            Dictionary<string, string[]> allHeaders = new Dictionary<string, string[]>(headers.Count);

            for (var i = 0; i < headers.Count; i++)
            {
                var headerValues = headers.GetValues(i);
                var key = headers.GetKey(i);
                allHeaders.Add(key, headerValues);
            }

            return allHeaders;
        }
    }

    public class ILogModule : IHttpModule
    {
        public void Dispose()
        {

        }
        public void Init(HttpApplication context)
        {
            context.Error += Context_Error;
            context.LogRequest += Context_LogRequest;
        }

        private void Context_Error(object sender, EventArgs e)
        {
            var app = sender as HttpApplication;
            var exception = app.Context.Server.GetLastError();
            var httpException = exception as HttpException;

            if (app.Context.IsCustomErrorEnabled)
            {
                if (httpException != null)
                {
                    var statusCode = httpException.GetHttpCode();
                    app.Server.ClearError();
                    app.Response.StatusCode = statusCode;
                    app.Context.Items.Add("PassThroughException", httpException);
                }
                else if (exception != null)
                {
                    app.Server.ClearError();
                    app.Response.StatusCode = 500;
                    app.Context.Items.Add("PassThroughException", exception);
                }
            }

        }

        private void Context_LogRequest(object sender, EventArgs e)
        {
            
            var app = sender as HttpApplication;
            var context = app.Context;

            var error = context?.Error ?? context.Items["PassThroughException"] as Exception;
            if (error != null)
            {

                var httperror = error as HttpException;
                if (httperror != null)
                {
                    var errorCode = httperror.GetHttpCode();
                    if (errorCode >= 500)
                    {
                        Log.Error(httperror, httperror.Message);
                    }
                    else if (errorCode >= 400)
                    {
                        Log.Warning(httperror, httperror.Message);
                    }
                }
                else
                {
                    Log.Error(error, error.Message);
                }
            }
            else if (context.Response.StatusCode >= 500)
            {
                Log.Error("Request failed");
            }
            else if (context.Response.StatusCode >= 400)
            {
                Log.Warning("Request failed");
            }
            else
            {
                Log.Information("Request succeeded");
            }

        }
    }
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            Log.Logger = new Serilog.LoggerConfiguration()

                .WriteTo.Trace(new Serilog.Formatting.Json.JsonFormatter()).Enrich.With<RequeUriEnricher>()
                .WriteTo.File(new Serilog.Formatting.Json.JsonFormatter(), "c:\\logs\\log.json").
                WriteTo.Elasticsearch(new Serilog.Sinks.Elasticsearch.ElasticsearchSinkOptions(new Uri("http://localhost:9200/")) { AutoRegisterTemplate = true })
                .CreateLogger();



            AreaRegistration.RegisterAllAreas();
            GlobalConfiguration.Configure(WebApiConfig.Register);
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
        }
        protected void Application_LogRequest()
        {
        }
    }
}
