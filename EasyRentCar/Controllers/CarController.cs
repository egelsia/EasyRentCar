using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using EasyRentCar.Models.EntityFramework;

namespace EasyRentCar.Controllers
{
    public class CarController : Controller
    {
        carDBEntities db = new carDBEntities();
        // GET: Car
        public ActionResult Index()
        {
            var carList = db.CARs.ToList();
            var model = carList.OrderBy(m => m.CAR_ID).ToList();
            return View(model);
        }
    }
}