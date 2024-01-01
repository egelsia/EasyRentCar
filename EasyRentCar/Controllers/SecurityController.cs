using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using EasyRentCar.Models.EntityFramework;

namespace EasyRentCar.Controllers
{
    public class SecurityController : Controller
    {
        internal carDBEntities db = new carDBEntities();
        // GET: Security
        public ActionResult Login()
        {
            
            return View();
        }

        [ValidateAntiForgeryToken]
        [HttpPost]
        public ActionResult Login(ADMIN admin)
        {
            var dbAdmin = db.ADMINs.FirstOrDefault(x => x.USERNAME == admin.USERNAME);
            if (dbAdmin != null)
            {
                string hashedPW = HashPassword(admin.PASSWORD, dbAdmin.PASSWORDSALT);

                if (hashedPW == dbAdmin.PASSWORD)
                {
                    FormsAuthentication.SetAuthCookie(dbAdmin.USERNAME, false);
                    return RedirectToAction("Index", "Home");
                }
                else
                {
                    ViewBag.Msg2 = "Wrong Password";
                    return View();
                }

            }
            ViewBag.Msg = "USER CAN NOT BE FOUND!";
            return View();
        }
        [Authorize]
        public ActionResult Logout()
        {
            FormsAuthentication.SignOut();
            return RedirectToAction("Index", "Home");
        }

        [Authorize]
        public ActionResult Register()
        {
            return View();
        }

        [Authorize]
        [ValidateAntiForgeryToken]
        [HttpPost]
        public ActionResult Register(ADMIN admin)
        {
            // Generate a random salt for the user
            string salt = GenerateSalt();

            // Hash the user's password with the generated salt
            string hashedPassword = HashPassword(admin.PASSWORD, salt);

            // Store the hashed password and salt in the database
            admin.PASSWORD = hashedPassword;
            admin.PASSWORDSALT = salt;
            db.ADMINs.Add(admin);
            db.SaveChanges();

            // Registration successful, redirect to a success page or login page
            return RedirectToAction("Login");
        }

        internal string GenerateSalt()
        {
            byte[] saltBytes = new byte[16]; // 16 bytes (128 bits) for salt
            using (var rng = new RNGCryptoServiceProvider())
            {
                rng.GetBytes(saltBytes);
            }
            return Convert.ToBase64String(saltBytes);
        }


        internal string HashPassword(string password, string salt)
        {
            byte[] saltBytes = System.Text.Encoding.UTF8.GetBytes(salt);
            var pbkdf2 = new Rfc2898DeriveBytes(password, saltBytes, 10000, System.Security.Cryptography.HashAlgorithmName.SHA256);
            byte[] hash = pbkdf2.GetBytes(32);

            return Convert.ToBase64String(hash);
        }

    }
}