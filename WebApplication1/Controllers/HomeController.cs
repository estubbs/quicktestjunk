using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace WebApplication1.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }
        public ActionResult Error()
        {
            throw new InvalidOperationException("fucked up");
            return Content("test");
        }
        public ActionResult NotFound()
        {
            return HttpNotFound();
        }
        public ActionResult BadRequest()
        {
            ModelState.AddModelError("thing", "its bad");
            return new HttpStatusCodeResult(400);
        }
        public ActionResult Redirect()
        {
            return new RedirectResult("/contact", false);
        }
    }
}