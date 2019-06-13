using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace PaymentGateway.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {

            ViewBag.Title = "Home Page";
            //int i = 0;
            //int j = 1;
            //var k = j / i;

            return View();
        }
    }
}
