using System;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Web;
using System.Web.Mvc;
using DogHub.Models;
using DogHub.BL;
using DogHub.HelpingClasses;
using Microsoft.Owin.Security;

namespace DogHub.Controllers
{
    public class AuthController : Controller
    {
        private readonly DogHubEntities db = new DogHubEntities();
        private readonly GeneralPurpose gp = new GeneralPurpose();

        #region Login
        [AllowAnonymous]
        public ActionResult Login(string msg = "", string color = "black")
        {
            if (gp.ValidateLoggedinUser() != null)
            {
                return RedirectToAction("Index", "DogBreed");
            }

            ViewBag.Message = msg;
            ViewBag.Color = color;
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        public ActionResult PostLogin(string Email = "", string Password = "")
        {
            try
            {
                var user = new UserBL().GetActiveUsers(db)
                            .FirstOrDefault(x => x.Email == Email && StringCipher.Decrypt(x.Password).Equals(Password));

                if (user == null)
                {
                    return RedirectToAction("Login", new { msg = "Incorrect Email/Password!", color = "red" });
                }

                //var identity = new ClaimsIdentity(new[]
                //{
                //    new Claim(ClaimTypes.Sid, user.PK_UserId.ToString()),
                //    new Claim(ClaimTypes.Email, user.Email),
                //    new Claim("UserName", user.UserName),
                //    new Claim("IsAdmin", (user.IsAdmin ?? false) ? "true" : "false")
                //}, "ApplicationCookie");


                var identity = new ClaimsIdentity(new[] {
                    new Claim(ClaimTypes.Name, user.UserName),
                    new Claim(ClaimTypes.Sid, user.PK_UserId.ToString()),
                    new Claim(ClaimTypes.Email, user.Email),
                    new Claim("UserName", user.UserName),
                    new Claim(ClaimTypes.Role, user.IsAdmin == true ?  "Admin" : "User")
                }, "ApplicationCookie");

                var claimsPrincipal = new ClaimsPrincipal(identity);
                Thread.CurrentPrincipal = claimsPrincipal;

                var ctx = Request.GetOwinContext();
                var authManager = ctx.Authentication;
                //authManager.SignIn(identity);
                authManager.SignIn(new AuthenticationProperties
                {
                    IsPersistent = true, // This remembers the login
                    ExpiresUtc = DateTime.UtcNow.AddDays(7) // Optional: expire in 7 days
                }, identity);

                return RedirectToAction("Index", "DogBreed");
            }
            catch
            {
                return RedirectToAction("Login", new { msg = "Unexpected error occurred.", color = "red" });
            }
        }
        #endregion

        #region Register
        [AllowAnonymous]
        public ActionResult Register(string msg = "", string color = "black")
        {
            ViewBag.Message = msg;
            ViewBag.Color = color;
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        public ActionResult PostRegister(User user, string ConfirmPassword = "")
        {
            try
            {
                if (user.Password != ConfirmPassword)
                {
                    return RedirectToAction("Register", new { msg = "Passwords do not match", color = "red" });
                }

                if (!gp.ValidateEmail(user.Email))
                {
                    return RedirectToAction("Register", new { msg = "Email already exists", color = "red" });
                }

                user.Password = StringCipher.Encrypt(user.Password.Trim());
                user.SysCreatedDate = DateTime.Now;
                user.IsActive = true;
                user.IsAdmin = false;

                bool success = new UserBL().AddUser(user, db);

                if (success)
                {
                    return RedirectToAction("Login", new { msg = "Registration successful! Please login.", color = "green" });
                }

                return RedirectToAction("Register", new { msg = "Something went wrong", color = "red" });
            }
            catch
            {
                return RedirectToAction("Register", new { msg = "Unexpected error occurred", color = "red" });
            }
        }
        #endregion

        #region ForgotPassword
        // GET: Auth/ForgotPassword
        [AllowAnonymous]
        public ActionResult ForgotPassword(string msg = "", string color = "black")
        {
            ViewBag.Message = msg;
            ViewBag.Color = color;
            return View();
        }

        // POST: Auth/ForgotPassword
        [HttpPost]
        [AllowAnonymous]
        public ActionResult ForgotPassword(string email)
        {
            try
            {
                User user = new UserBL().GetActiveUsers(db).FirstOrDefault(x => x.Email.ToLower() == email.ToLower());
                if (user == null)
                {
                    return RedirectToAction("ForgotPassword", new { msg = "Email not found!", color = "red" });
                }

                bool isSent = MailSender.SendForgotPasswordEmail(email, Request.Url.GetLeftPart(UriPartial.Authority) + "/Auth/ForgotPassword");
                if (isSent)
                {
                    return RedirectToAction("Login", new { msg = "Reset link sent to your email", color = "green" });
                }
                else
                {
                    return RedirectToAction("ForgotPassword", new { msg = "Email sending failed. Try again later!", color = "red" });
                }
            }
            catch
            {
                return RedirectToAction("Index", "Error");
            }
        }
        #endregion

        #region ResetPassword
        // GET: Auth/ResetPassword
        [AllowAnonymous]
        public ActionResult ResetPassword(string email, string time)
        {
            ViewBag.Email = email;
            ViewBag.Time = time;

            // Decode time
            var requestTime = DateTime.Parse(StringCipher.Base64Decode(time));
            if ((DateTime.Now - requestTime).TotalHours > 24)
            {
                ViewBag.Message = "Reset link expired!";
                ViewBag.Color = "red";
                return View();
            }

            return View();
        }

        // POST: Auth/ResetPassword
        [HttpPost]
        [AllowAnonymous]
        public ActionResult ResetPassword(string email, string time, string newPassword, string confirmPassword)
        {
            try
            {
                if (newPassword != confirmPassword)
                {
                    ViewBag.Message = "Passwords do not match!";
                    ViewBag.Color = "red";
                    return View();
                }

                string decodedEmail = StringCipher.Base64Decode(email);
                User user = new UserBL().GetActiveUsers(db).FirstOrDefault(x => x.Email == decodedEmail);
                if (user == null)
                {
                    ViewBag.Message = "Invalid email!";
                    ViewBag.Color = "red";
                    return View();
                }

                user.Password = StringCipher.Encrypt(newPassword);
                new UserBL().UpdateUser(user, db);

                return RedirectToAction("Login", new { msg = "Password reset successfully. Try login!", color = "green" });
            }
            catch
            {
                ViewBag.Message = "Something went wrong!";
                ViewBag.Color = "red";
                return View();
            }
        }
        #endregion

        #region Logout
        [AllowAnonymous]
        public ActionResult Logout()
        {
            var ctx = Request.GetOwinContext();
            var authManager = ctx.Authentication;
            authManager.SignOut("ApplicationCookie");
            return RedirectToAction("Login");
        }
        #endregion
    }
}
