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
using System.Data.Entity.Migrations;
using System.IO;

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
            var mockSet = new Mock<DbSet<CAR>>();

            // Setup the DbSet
            mockDbContext.Setup(m => m.CARs).Returns(mockSet.Object);

            // Setup for Add method if Save uses it to add a new car
            mockSet.Setup(m => m.Add(It.IsAny<CAR>())).Returns((CAR c) => c);

            _controller = new CarController();
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

            // If Save method calls SaveChanges on the DbContext
            mockDbContext.Setup(m => m.SaveChanges()).Verifiable();

            // Act
            var result = _controller.Save(carToCreate, null) as RedirectToRouteResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual("Index", result.RouteValues["action"]);
            mockDbContext.Verify(m => m.SaveChanges(), Times.Once); // Verify that SaveChanges was called
        }

        [TestMethod]
        public void Save_Update_ValidCar_RedirectsToIndex()
        {
            var mockDbContext = new Mock<carDBEntities>();
            var mockSet = new Mock<DbSet<CAR>>();

            var carToUpdate = new CAR
            {
                CAR_ID = 2,
                CAR_BRAND = "UpdatedBrand",
                CAR_MODEL = "UpdatedModel",
                // Other properties as needed
            };

            // Setup the DbSet and its methods
            mockDbContext.Setup(m => m.CARs).Returns(mockSet.Object);
            mockSet.Setup(m => m.Find(carToUpdate.CAR_ID)).Returns(carToUpdate);

            _controller = new CarController();
            _controller.db = mockDbContext.Object;

            // Mocking HttpPostedFileBase if needed for file upload test
            var mockFile = new Mock<HttpPostedFileBase>();
            mockFile.Setup(f => f.ContentLength).Returns(1);
            mockFile.Setup(f => f.InputStream).Returns(new MemoryStream(new byte[1]));

            // If Save method calls SaveChanges on the DbContext
            mockDbContext.Setup(m => m.SaveChanges()).Verifiable();

            // Act
            var result = _controller.Save(carToUpdate, mockFile.Object) as RedirectToRouteResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual("Index", result.RouteValues["action"]);
            mockDbContext.Verify(m => m.SaveChanges(), Times.Once); // Verify that SaveChanges was called
        }

        [TestMethod]
        public void Edit_ReturnsCarFormWithModel()
        {
            var mockDbContext = new Mock<carDBEntities>();
            var mockSet = new Mock<DbSet<CAR>>();

            var carId = 1;
            var car = new CAR { CAR_ID = carId, CAR_BRAND = "Brand", /* Other properties */ };

            mockDbContext.Setup(m => m.CARs).Returns(mockSet.Object);
            mockSet.Setup(m => m.Find(carId)).Returns(car);

            _controller = new CarController();
            _controller.db = mockDbContext.Object;

            // Act
            var result = _controller.Edit(carId) as ViewResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual("CarForm", result.ViewName);
            Assert.AreEqual(car, result.Model);
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

        [TestMethod]
        public void FilterCars_WithBrandFilter_ReturnsFilteredCars()
        {
            // Arrange
            var mockDbContext = new Mock<carDBEntities>();
            var controller = new CarController();
            controller.ControllerContext = new ControllerContext
            {
                HttpContext = HttpContextMock.CreateMockHttpContext(true) // Simulate an authenticated user
            };
            controller.db = mockDbContext.Object;

            var cars = new List<CAR>
            {
                new CAR { CAR_BRAND = "Toyota" },
                new CAR { CAR_BRAND = "Honda" },
                new CAR { CAR_BRAND = "Ford" }
            };

            var mockSet = CreateDbSetMock(cars);

            mockDbContext.Setup(db => db.CARs).Returns(mockSet.Object);

            // Act
            var result = controller.FilterCars("Toyota", null, null, null, null, null, null) as ViewResult;
            var model = result.Model as List<CAR>;

            // Assert
            Assert.IsNotNull(model);
            Assert.AreEqual(1, model.Count);
            Assert.AreEqual("Toyota", model[0].CAR_BRAND);
        }

        [TestMethod]
        public void FilterCars_WithNoFilters_ReturnsAllCars()
        {
            // Arrange
            var mockDbContext = new Mock<carDBEntities>();
            var controller = new CarController();
            controller.ControllerContext = new ControllerContext
            {
                HttpContext = HttpContextMock.CreateMockHttpContext(true) // Simulate an authenticated user
            };
            controller.db = mockDbContext.Object;

            var cars = new List<CAR>
            {
                new CAR { CAR_BRAND = "Toyota" },
                new CAR { CAR_BRAND = "Honda" },
                new CAR { CAR_BRAND = "Ford" }
            };

            var mockSet = CreateDbSetMock(cars);
            mockDbContext.Setup(db => db.CARs).Returns(mockSet.Object);

            // Act
            var result = controller.FilterCars(null, null, null, null, null, null, null) as ViewResult;
            var model = result.Model as List<CAR>;

            // Assert
            Assert.IsNotNull(model);
            Assert.AreEqual(3, model.Count);
        }

        [TestMethod]
        public void FilterCars_UserNotAuthenticated_ReturnsAvailableCars()
        {
            // Arrange
            var mockDbContext = new Mock<carDBEntities>();
            var controller = new CarController();
            controller.ControllerContext = new ControllerContext
            {
                HttpContext = HttpContextMock.CreateMockHttpContext(false) // Simulate an unauthenticated user
            };
            controller.db = mockDbContext.Object;

            var cars = new List<CAR>
            {
                new CAR { CAR_BRAND = "Toyota", CAR_AVAILABLE = true },
                new CAR { CAR_BRAND = "Honda", CAR_AVAILABLE = false },
                new CAR { CAR_BRAND = "Ford", CAR_AVAILABLE = true }
            };

            var mockSet = CreateDbSetMock(cars);
            mockDbContext.Setup(db => db.CARs).Returns(mockSet.Object);

            // Act
            var result = controller.FilterCars(null, null, null, null, null, null, null) as ViewResult;
            var model = result.Model as List<CAR>;

            // Assert
            Assert.IsNotNull(model);
            Assert.AreEqual(2, model.Count);
            Assert.IsTrue(model.All(c => c.CAR_AVAILABLE));
        }

        private static Mock<DbSet<T>> CreateDbSetMock<T>(List<T> elements) where T : class
        {
            var elementsAsQueryable = elements.AsQueryable();
            var dbSetMock = new Mock<DbSet<T>>();
            dbSetMock.As<IQueryable<T>>().Setup(m => m.Provider).Returns(elementsAsQueryable.Provider);
            dbSetMock.As<IQueryable<T>>().Setup(m => m.Expression).Returns(elementsAsQueryable.Expression);
            dbSetMock.As<IQueryable<T>>().Setup(m => m.ElementType).Returns(elementsAsQueryable.ElementType);
            dbSetMock.As<IQueryable<T>>().Setup(m => m.GetEnumerator()).Returns(elementsAsQueryable.GetEnumerator());
            return dbSetMock;
        }
    }

    [TestClass]
    public class ErrorControllerTests
    {
        [TestMethod]
        public void NotFound_ReturnsView()
        {
            // Arrange
            var controller = new ErrorController();

            // Act
            var result = controller.NotFound() as ViewResult;

            // Assert
            Assert.IsNotNull(result);
        }
    }

    [TestClass]
    public class HomeControllerTests
    {
        private HomeController _controller;

        [TestMethod]
        public void Index_ReturnsViewWithModel()
        {
            // Arrange
            var mockDbContext = new Mock<carDBEntities>();

            // Ensure all necessary properties and dependencies of HomeController are initialized
            _controller = new HomeController();
            _controller.db = mockDbContext.Object;

            var cars = new List<CAR>
            {
                new CAR { CAR_BRAND = "Toyota" },
                new CAR { CAR_BRAND = "Honda" },
                new CAR { CAR_BRAND = "Ford" }
            };

            var mockSet = CreateDbSetMock(cars);
            mockDbContext.Setup(db => db.CARs).Returns(mockSet.Object);

            // Act
            var result = _controller.Index();

            // Assert
            Assert.IsNotNull(result, "Result is null");
            Assert.IsInstanceOfType(result, typeof(ViewResult), "Result is not a ViewResult");

            var viewResult = result as ViewResult;
            Assert.IsNotNull(viewResult.Model, "Model is null");
        }

        private static Mock<DbSet<T>> CreateDbSetMock<T>(List<T> elements) where T : class
        {
            var elementsAsQueryable = elements.AsQueryable();
            var dbSetMock = new Mock<DbSet<T>>();
            dbSetMock.As<IQueryable<T>>().Setup(m => m.Provider).Returns(elementsAsQueryable.Provider);
            dbSetMock.As<IQueryable<T>>().Setup(m => m.Expression).Returns(elementsAsQueryable.Expression);
            dbSetMock.As<IQueryable<T>>().Setup(m => m.ElementType).Returns(elementsAsQueryable.ElementType);
            dbSetMock.As<IQueryable<T>>().Setup(m => m.GetEnumerator()).Returns(elementsAsQueryable.GetEnumerator());
            return dbSetMock;
        }

        [TestMethod]
        public void Contact_ReturnsView() {
        
            var result = _controller.Contact() as ViewResult;

            // Assert
            Assert.IsNotNull(result);
        }
    }
}
