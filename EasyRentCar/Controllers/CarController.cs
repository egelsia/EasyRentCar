using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Migrations;
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

        public ActionResult Details(int id)
        {
            var car = db.CARs.Find(id);

            return View(car);
        }

        [Authorize]
        public ActionResult Create()
        {
            return View("CarForm");
        }

        [Authorize]
        [ValidateAntiForgeryToken]
        [HttpPost]
        public ActionResult Save(CAR model, HttpPostedFileBase file)
        {

            if (file != null && file.ContentLength > 0)
            {
                byte[] imageData = null;
                using (var binaryReader = new System.IO.BinaryReader(file.InputStream))
                {
                    imageData = binaryReader.ReadBytes(file.ContentLength);
                }

                model.CAR_IMG = imageData;
            }
            else if (model.CAR_ID != 0)
            {
                CAR modelcar = db.CARs.Find(model.CAR_ID);
                model.CAR_IMG = modelcar.CAR_IMG;

            }

            if (model.CAR_ID == 0)
            {
                db.CARs.Add(model);
            }
            else
            {

                db.Entry(model).State = EntityState.Modified;
            }

            db.SaveChanges();
            return RedirectToAction("Index", "Car");
        }

        [Authorize]
        public ActionResult Edit(int id)
        {
            var model = db.CARs.Find(id);
            return View("CarForm", model);
        }

        [Authorize]
        public ActionResult Delete(int id)
        {
            var carToBeDeleted = db.CARs.Find(id);
            if (carToBeDeleted == null) { return HttpNotFound(); }
            db.CARs.Remove(carToBeDeleted);
            db.SaveChanges();
            return RedirectToAction("Index", "Car");
        }

        public ActionResult DisplayImage(int id)
        {
            var model = db.CARs.Find(id);

            if (model.CAR_IMG != null)
            {
                return File(model.CAR_IMG, "image/png"); // You can adjust the content type based on your image types
            }

            // Handle error case or show a default image
            return File("~/Content/img-not-available.jpg", "image/jpeg");
        }

    }
}