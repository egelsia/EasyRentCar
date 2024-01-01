using Microsoft.VisualStudio.TestTools.UnitTesting;
using EasyRentCar.Controllers;
using EasyRentCar.Models.EntityFramework;
using Moq;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Reflection;
using System.Security.Principal;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;

namespace EasyRentCar.Tests.Controllers
{
    [TestClass]
    public class CarControllerTests
    {
        private CarController _controller;
        private Mock<carDBEntities> _mockContext;
        private Mock<DbSet<CAR>> _mockSet;

        [TestInitialize]
        public void SetUp()
        {
            var carData = new List<CAR>
            {
                new CAR { CAR_ID = 1, CAR_BRAND = "Toyota" },
                new CAR { CAR_ID = 2, CAR_BRAND = "Honda" },
                new CAR { CAR_ID = 3, CAR_BRAND = "Toyota", CAR_MODEL = "Model1", CAR_PRICE = 100, CAR_SEATS = 4, CAR_TRANSMISSION = true, CAR_FUEL = "Gasoline", CAR_DOORS = 4 }
            }.AsQueryable();

            _mockSet = new Mock<DbSet<CAR>>();
            _mockSet.As<IQueryable<CAR>>().Setup(m => m.Provider).Returns(carData.Provider);
            _mockSet.As<IQueryable<CAR>>().Setup(m => m.Expression).Returns(carData.Expression);
            _mockSet.As<IQueryable<CAR>>().Setup(m => m.ElementType).Returns(carData.ElementType);
            _mockSet.As<IQueryable<CAR>>().Setup(m => m.GetEnumerator()).Returns(carData.GetEnumerator());

            _mockContext = new Mock<carDBEntities>();
            _mockContext.Setup(c => c.CARs).Returns(_mockSet.Object);

            _controller = new CarController();

            var dbField = typeof(CarController).GetField("db", BindingFlags.NonPublic | BindingFlags.Instance);
            dbField.SetValue(_controller, _mockContext.Object);

            var mockHttpContext = new Mock<HttpContextBase>();
            var mockIdentity = new Mock<IIdentity>();
            mockHttpContext.SetupGet(x => x.User.Identity).Returns(mockIdentity.Object);
            mockIdentity.Setup(x => x.IsAuthenticated).Returns(true);

            _controller.ControllerContext = new ControllerContext(mockHttpContext.Object, new System.Web.Routing.RouteData(), _controller);
        }

        [TestMethod]
        public void Index_WhenCalled_ReturnsViewWithCars()
        {
            var result = _controller.Index() as ViewResult;

            Assert.IsNotNull(result);
            var viewResult = (ViewResult)result;
            var model = (List<CAR>)viewResult.Model;
            Assert.AreEqual(3, model.Count);
        }

        [TestMethod]
        public void Index_WhenCalledByUnauthenticatedUser_ReturnsAvailableCars()
        {
            var mockIdentity = new Mock<IIdentity>();
            mockIdentity.Setup(x => x.IsAuthenticated).Returns(false);
            var mockHttpContext = new Mock<HttpContextBase>();
            mockHttpContext.SetupGet(x => x.User.Identity).Returns(mockIdentity.Object);
            _controller.ControllerContext = new ControllerContext(mockHttpContext.Object, new System.Web.Routing.RouteData(), _controller);

            var carData = new List<CAR>
            {
                new CAR { CAR_ID = 1, CAR_BRAND = "Toyota", CAR_AVAILABLE = true },
                new CAR { CAR_ID = 2, CAR_BRAND = "Honda", CAR_AVAILABLE = false }
            }.AsQueryable();

            _mockSet.As<IQueryable<CAR>>().Setup(m => m.Provider).Returns(carData.Provider);
            _mockSet.As<IQueryable<CAR>>().Setup(m => m.Expression).Returns(carData.Expression);
            _mockSet.As<IQueryable<CAR>>().Setup(m => m.ElementType).Returns(carData.ElementType);
            _mockSet.As<IQueryable<CAR>>().Setup(m => m.GetEnumerator()).Returns(carData.GetEnumerator());

            var result = _controller.Index() as ViewResult;
            var model = result.Model as List<CAR>;

            Assert.IsNotNull(result);
            Assert.AreEqual(1, model.Count); 
        }

        [TestMethod]
        public void Index_WhenCalled_ReturnsViewWithCarsIncludingModelImageAndPrice()
        {
            var carData = new List<CAR>
            {
                new CAR { CAR_ID = 1, CAR_MODEL = "Model1", CAR_IMG = new byte[0], CAR_PRICE = 100, CAR_AVAILABLE = true },
                new CAR { CAR_ID = 2, CAR_MODEL = "Model2", CAR_IMG = new byte[0], CAR_PRICE = 150, CAR_AVAILABLE = true }
            }.AsQueryable();

            _mockSet.As<IQueryable<CAR>>().Setup(m => m.Provider).Returns(carData.Provider);
            _mockSet.As<IQueryable<CAR>>().Setup(m => m.Expression).Returns(carData.Expression);
            _mockSet.As<IQueryable<CAR>>().Setup(m => m.ElementType).Returns(carData.ElementType);
            _mockSet.As<IQueryable<CAR>>().Setup(m => m.GetEnumerator()).Returns(carData.GetEnumerator());

            _mockContext.Setup(c => c.CARs).Returns(_mockSet.Object);

            var result = _controller.Index() as ViewResult;

            Assert.IsNotNull(result);
            var viewResult = (ViewResult)result;
            var model = viewResult.Model as List<CAR>;
            Assert.IsNotNull(model);
            Assert.AreEqual(2, model.Count);

            foreach (var car in model)
            {
                Assert.IsFalse(string.IsNullOrEmpty(car.CAR_MODEL));
                Assert.IsNotNull(car.CAR_IMG);
                Assert.IsTrue(car.CAR_PRICE > 0);
            }
        }

        [TestMethod]
        public void Details_ValidCarId_ReturnsCarWithDetails()
        {
            var mockDbContext = new Mock<carDBEntities>();
            var carData = new CAR
            {
                CAR_ID = 3,
                CAR_BRAND = "Toyota",
                CAR_MODEL = "Model1",
                CAR_PRICE = 100,
                CAR_SEATS = 4,
                CAR_TRANSMISSION = true,
                CAR_FUEL = "Gasoline",
                CAR_DOORS = 4
            };
            mockDbContext.Setup(db => db.CARs.Find(3)).Returns(carData);

            var controller = new CarController();
            controller.db = mockDbContext.Object;

            var result = controller.Details(3, 1) as ViewResult;

            Assert.IsNotNull(result);
            var car = result.Model as CAR;
            Assert.IsNotNull(car);
            Assert.AreEqual("Toyota", car.CAR_BRAND);
            Assert.AreEqual("Model1", car.CAR_MODEL);
            Assert.AreEqual((short)100, car.CAR_PRICE);
            Assert.AreEqual((short)4, car.CAR_SEATS);
            Assert.IsTrue(car.CAR_TRANSMISSION);
            Assert.AreEqual("Gasoline", car.CAR_FUEL);
            Assert.AreEqual((short)4, car.CAR_DOORS);
        }

        [TestMethod]
        public void Save_Create_ValidCar_RedirectsToIndex()
        {
            var mockDbContext = new Mock<carDBEntities>();
            
            _controller.db = mockDbContext.Object;

            var carToCreate = new CAR
            {
                CAR_ID = 0,
                CAR_BRAND = "Toyota",
                CAR_MODEL = "Model1",
                CAR_PRICE = 100,
                CAR_SEATS = 4,
                CAR_TRANSMISSION = true,
                CAR_FUEL = "Gasoline",
                CAR_DOORS = 4
            };

            var result = _controller.Save(carToCreate, null) as RedirectToRouteResult;

            Assert.IsNotNull(result);
            Assert.AreEqual("Index", result.RouteValues["action"]);
        }

        [TestMethod]
        public void Save_Update_ValidCar_RedirectsToIndex()
        {
            var mockDbContext = new Mock<carDBEntities>();

            _controller.db = mockDbContext.Object;

            var carToUpdate = new CAR
            {
                CAR_ID = 1,
                CAR_BRAND = "UpdatedBrand",
                CAR_MODEL = "UpdatedModel",
                CAR_PRICE = 200,
                CAR_SEATS = 6,
                CAR_TRANSMISSION = false,
                CAR_FUEL = "Electric",
                CAR_DOORS = 2
            };

            var result = _controller.Save(carToUpdate, null) as RedirectToRouteResult;

            Assert.IsNotNull(result);
            Assert.AreEqual("Index", result.RouteValues["action"]);
        }

        [TestMethod]
        public void Delete_ValidCar_RedirectsToIndex()
        {
            var mockDbContext = new Mock<carDBEntities>();
            var controller = new CarController();
            controller.db = mockDbContext.Object;

            var carToDelete = new CAR
            {
                CAR_ID = 1,
                CAR_BRAND = "ToDeleteBrand",
                CAR_MODEL = "ToDeleteModel",
                CAR_PRICE = 150,
                CAR_SEATS = 5,
                CAR_TRANSMISSION = true,
                CAR_FUEL = "Motorin",
                CAR_DOORS = 4
            };

            mockDbContext.Setup(db => db.CARs.Find(1)).Returns(carToDelete);

            var result = controller.Delete(1) as RedirectToRouteResult;

            Assert.IsNotNull(result);
            Assert.AreEqual("Index", result.RouteValues["action"]);
        }
    }
}
