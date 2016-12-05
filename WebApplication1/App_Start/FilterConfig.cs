using System;
using System.Web;
using System.Web.Mvc;

namespace WebApplication1
{
    public class ErrorFileAttribute : FilterAttribute, IExceptionFilter
    {
        public readonly string _ErrorFilePath;
        public ErrorFileAttribute(string errorFilePath)
        {
            _ErrorFilePath = errorFilePath;
        }
        public void OnException(ExceptionContext filterContext)
        {
            if (filterContext.HttpContext.IsCustomErrorEnabled)
            {
                var exception = filterContext.Exception;
                
                filterContext.Result = new System.Web.Mvc.FilePathResult(_ErrorFilePath, "text/html");
                filterContext.ExceptionHandled = true;
                filterContext.HttpContext.Response.StatusCode = 400;
                filterContext.HttpContext.Response.TrySkipIisCustomErrors = true;
            }

        }

    }
    public class FilterConfig
    {
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
        //    filters.Add(new ErrorFileAttribute("\\errors\\500.html"));
        }
    }
}
