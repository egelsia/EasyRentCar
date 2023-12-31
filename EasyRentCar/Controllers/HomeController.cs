﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using EasyRentCar.Models.EntityFramework;

namespace EasyRentCar.Controllers
{
    public class HomeController : Controller
    {
        
        internal carDBEntities db = new carDBEntities();
        // GET: Home
        public ActionResult Index()
        {
            var carList = db.CARs.ToList();
            Random random = new Random();
            var model = carList.Where(m => m.CAR_AVAILABLE == true).OrderBy(x => random.Next()).Take(4).ToList();
            return View(model);
        }

        public ActionResult Contact()
        {

            return View();
        }
    }
}