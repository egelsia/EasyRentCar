using System;
using System.Collections.Generic;
using System.Data.Entity;
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

        [HttpPost]
        public ActionResult Index(List<CAR> carList)
        {
            return View(carList);
        }

        // GET: Car/Details/5
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

        public ActionResult FilterCars(string brand, string model, int? minPrice, int? maxPrice, int? seats, string fuelType, bool? transmissionType)
        {
            IQueryable<CAR> query = db.CARs;

            if (!string.IsNullOrEmpty(brand))
            {
                query = query.Where(c => c.CAR_BRAND.ToLower().Contains(brand.ToLower()));
            }
            if (!string.IsNullOrEmpty(model))
            {
                query = query.Where(c => c.CAR_MODEL.ToLower().Contains(model.ToLower()));
            }
            if (minPrice.HasValue)
            {
                query = query.Where(c => c.CAR_PRICE >= minPrice.Value);
            }
            if (maxPrice.HasValue)
            {
                query = query.Where(c => c.CAR_PRICE <= maxPrice.Value);
            }
            if (seats.HasValue)
            {
                query = query.Where(c => c.CAR_SEATS == seats.Value);
            }
            if (transmissionType != null)
            {
                query = query.Where(c => c.CAR_TRANSMISSION == transmissionType);
            }
            if (!string.IsNullOrEmpty(fuelType))
            {
                query = query.Where(c => c.CAR_FUEL == fuelType);
            }

            List<CAR> carList = query.ToList();

            return View("Index", carList);
        }

    }
}