using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using EasyRentCar.Models.EntityFramework;

namespace EasyRentCar.Controllers
{
    public class HomeController : Controller
    {
        
        carDBEntities db = new carDBEntities();
        // GET: Home
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Contact()
        {

            return View();
        }
    }
}