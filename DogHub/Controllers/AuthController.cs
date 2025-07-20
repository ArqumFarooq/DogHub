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
                if (gp.ValidateLoggedinUser().IsAdmin == true)
                {
                    return RedirectToAction("Dashboard", "Admin");
                }

                return RedirectToAction("Dashboard", "Admin");
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
                var user = new UserBL().GetActiveUsers(db).Where(x => x.Email == Email && StringCipher.Decrypt(x.Password).Equals(Password)).FirstOrDefault();

                if (user == null)
                {
                    return RedirectToAction("Login", new { msg = "Incorrect Email/Password!", color = "red" });
                }

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
                authManager.SignIn(new AuthenticationProperties
                {
                    IsPersistent = true, // This remembers the login
                    ExpiresUtc = DateTime.UtcNow.AddDays(2) // Optional: expire in 2 days
                }, identity);

                if(user.IsAdmin == true)
                {
                    return RedirectToAction("Dashboard", "Admin");
                }
                else
                {
                    return RedirectToAction("Dashboard", "Admin");
                    //return RedirectToAction("Login", "Auth", new { msg = "Only User is allowed to login", color = "red" });
                }
                
            }
            catch
            {
                //return RedirectToAction("Index", "Error");
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
        public ActionResult PostRegister(User user, string ConfirmEmail = "", string ConfirmPassword = "")
        {
            try
            {
                if (user.Password != ConfirmPassword)
                {
                    return RedirectToAction("Register", "Auth", new { msg = "Passwords do not match", color = "red" });
                }
                if (user.Email.ToLower() != ConfirmEmail.ToLower())
                {
                    return RedirectToAction("Register", "Auth", new { msg = "Emails do not match", color = "red" });
                }

                if (!gp.ValidateEmail(user.Email))
                {
                    return RedirectToAction("Register", "Auth", new { msg = "Email already exist, Try another!", color = "red" });
                }

                user.Password = StringCipher.Encrypt(user.Password.Trim());
                user.SysCreatedDate = DateTime.Now;
                user.IsActive = true;
                user.IsAdmin = false;

                bool success = new UserBL().AddUser(user, db);

                if (success)
                {
                    return RedirectToAction("Login", "Auth", new { msg = "Registration successful! Please login.", color = "green" });
                }

                return RedirectToAction("Register", "Auth", new { msg = "Something went wrong", color = "red" });
            }
            catch
            {
                return RedirectToAction("Register", "Auth", new { msg = "Unexpected error occurred", color = "red" });
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
        public ActionResult PostForgotPassword(string email)
        {
            try
            {
                User user = new UserBL().GetActiveUsers(db).FirstOrDefault(x => x.Email.ToLower() == email.ToLower());
                if (user == null)
                {
                    return RedirectToAction("ForgotPassword", "Auth", new { msg = "Email not found!", color = "red" });
                }

                bool isSent = MailSender.SendForgotPasswordEmail(email, Request.Url.GetLeftPart(UriPartial.Authority) + "/Auth/ResetPassword?email=" + StringCipher.Base64Encode(email) + "&time=" + StringCipher.Base64Encode(DateTime.Now.ToString("MM/dd/yyyy")));
                if (isSent)
                {
                    return RedirectToAction("Login", "Auth", new { msg = "Reset link sent to your email", color = "green" });
                }
                else
                {
                    return RedirectToAction("ForgotPassword", "Auth", new { msg = "Email sending failed. Try again later!", color = "red" });
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
        public ActionResult ResetPassword(string email = "", string time = "", string msg = "", string color = "black")
        {
            try
            {
                DateTime PassDate = Convert.ToDateTime(StringCipher.Base64Decode(time)).Date;
                DateTime CurrentDate = DateTime.Now.Date;
                if (CurrentDate != PassDate)
                {
                    return RedirectToAction("Login", "Auth", new { msg = "Link expired, Please try again!", color = "red" });
                }

                ViewBag.Time = time;
                ViewBag.Email = email;
                ViewBag.Message = msg;
                ViewBag.Color = color;

                return View();
            }
            catch
            {
                return RedirectToAction("Index", "Error");
            }
        }

        // POST: Auth/ResetPassword
        [HttpPost]
        [AllowAnonymous]
        public ActionResult PostResetPassword(string Email = "", string Time = "", string newPassword = "", string ConfirmPassword = "")
        {
            try
            {
                if (newPassword != ConfirmPassword)
                {
                    return RedirectToAction("ResetPassword", "Auth", new { email = Email, time = Time, msg = "Password and confirm password did not match", color = "red" });
                }

                string DecryptEmail = StringCipher.Base64Decode(Email);
                User user = new UserBL().GetActiveUsers(db).Where(x => x.Email.Trim().ToLower() == DecryptEmail.Trim().ToLower()).FirstOrDefault();
                user.Password = StringCipher.Encrypt(newPassword);
                bool check = new UserBL().UpdateUser(user, db);

                if (check == true)
                {
                    return RedirectToAction("Login", "Auth", new { msg = "Password reset successful, Try Login", color = "green" });
                }
                else
                {
                    return RedirectToAction("ResetPassword", "Auth", new { email = Email, time = Time, msg = "Somethings' Wrong!", color = "red" });
                }
            }
            catch
            {
                return RedirectToAction("Index", "Error");
            }
        }
        #endregion

        #region Manage Profile
        [AllowAnonymous]
        public ActionResult UpdateProfile(string msg = "", string color = "black")
        {
            if (gp.ValidateLoggedinUser() == null)
            {
                return RedirectToAction("Login", "Auth", new { msg = "Session Expired! Please Login", color = "red" });
            }

            ViewBag.Message = msg;
            ViewBag.Color = color;

            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        public ActionResult PostUpdateProfile(User _user)
        {
            try
            {
                if (gp.ValidateLoggedinUser() == null)
                {
                    return RedirectToAction("Login", "Auth", new { msg = "Session Expired! Please Login", color = "red" });
                }
                bool chkEmail = gp.ValidateEmail(_user.Email, _user.PK_UserId);
                if (chkEmail == false)
                {
                    return RedirectToAction("UpdateProfile", "Auth", new { msg = "Email used by someone else, Please try another", color = "red" });
                }
                User user = new UserBL().GetUserById(_user.PK_UserId, db);
                user.UserName = _user.UserName.Trim();
                user.Email = _user.Email.Trim();
                user.Password = StringCipher.Encrypt(_user.Password.Trim());

                bool chkUser = new UserBL().UpdateUser(user, db);
                if (chkUser == true)
                {
                    return RedirectToAction("UpdateProfile", "Auth", new { msg = "Profile updated successfully!", color = "green" });
                }
                else
                {
                    return RedirectToAction("UpdateProfile", "Auth", new { msg = "Something' Wrong", color = "red" });
                }
            }
            catch
            {
                return RedirectToAction("Index", "Error");
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
