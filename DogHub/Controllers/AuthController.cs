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
using Microsoft.Owin.Security.Cookies;
using Microsoft.Owin.Security.OAuth;
using System.Threading.Tasks;

namespace DogHub.Controllers
{
    public class AuthController : Controller
    {
        private readonly DogHubEntities db = new DogHubEntities();
        private readonly GeneralPurpose gp = new GeneralPurpose();
        private IAuthenticationManager AuthenticationManager => HttpContext.GetOwinContext().Authentication;

        private void LogAudit(string actionType, string affectedTable, string details, int? userId = null)
        {
            var auditBL = new AuditLogBL();
            auditBL.AddAuditLog(
                actionType: actionType,
                affectedTable: affectedTable,
                auditDetails: details,
                createdById: userId,
                de: db
            );
        }

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
                    LogAudit("Login", "Users", $"Failed login attempt with email: {Email}", user.PK_UserId);
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
                    IsPersistent = true, // remember the login
                    ExpiresUtc = DateTime.UtcNow.AddDays(7) // expire in 7 days
                }, identity);

                if(user.IsAdmin == true)
                {
                    LogAudit("Login", "Users", $"Admin logged in with email: {Email}", user.PK_UserId);
                    return RedirectToAction("Dashboard", "Admin");
                }
                else
                {
                    LogAudit("Login", "Users", $"User logged in with email: {Email}", user.PK_UserId);
                    return RedirectToAction("Dashboard", "Admin");
                }
                
            }
            catch
            {
                LogAudit("Login", "Users", "Unexpected error occurred", 0);
                return RedirectToAction("Login", new { msg = "Unexpected error occurred.", color = "red" });
            }
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult ExternalLogin(string provider, string returnUrl)
        {
            return new ChallengeResult(
                provider,
                Url.Action("ExternalLoginCallback", "Auth", new { ReturnUrl = returnUrl })
            );
        }

        [AllowAnonymous]
        public async Task<ActionResult> ExternalLoginCallback(string returnUrl)
        {
            var loginInfo = await AuthenticationManager.GetExternalLoginInfoAsync();
            if (loginInfo == null)
            {
                LogAudit("External Login", "Users", "Google Login info not found", 0);
                return RedirectToAction("Login", "Auth", new { msg = "Google Login info not found. Please try again.", color = "red" });
            }

            var user = new UserBL().GetActiveUsers(db).Where(x => x.Email == loginInfo.Email).FirstOrDefault(); 
            if (user == null)
            {
                var newUser = new User
                {
                    UserName = loginInfo.DefaultUserName,
                    Email = loginInfo.Email,
                    IsActive = true,
                    IsAdmin = false,
                    AuthId = loginInfo.Login.ProviderKey,
                    AuthProvider = loginInfo.Login.LoginProvider,
                    SysCreatedDate = DateTime.Now

                };

                bool success =  new UserBL().AddUser(newUser, db);
                if (success) {
                    LogAudit("External Register", "Users", $"New user registered via  Google Credentials:{loginInfo.Email}", 0);
                }
                user = newUser;
            }

            var identity = new ClaimsIdentity(new[] {
            new Claim(ClaimTypes.Name, user.UserName),
            new Claim(ClaimTypes.Sid, user.PK_UserId.ToString()),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim("UserName", user.UserName),
            new Claim(ClaimTypes.Role, user.IsAdmin == true ?  "Admin" : "User")}, "ApplicationCookie");

            AuthenticationManager.SignIn(new AuthenticationProperties
            {
                IsPersistent = true, // remember the login
                ExpiresUtc = DateTime.UtcNow.AddDays(7) // expire in 7 days
            }, identity);

            return RedirectToAction("Dashboard", "Admin");
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
                    LogAudit("Register","Users", $"Failed Registeration attempt because Passwords do not match",0);
                    return RedirectToAction("Register", "Auth", new { msg = "Passwords do not match", color = "red" });
                }
                if (user.Email.ToLower() != ConfirmEmail.ToLower())
                {
                    LogAudit("Register", "Users", $"Failed Registeration attempt because Emails do not match", 0);
                    return RedirectToAction("Register", "Auth", new { msg = "Emails do not match", color = "red" });
                }

                if (!gp.ValidateEmail(user.Email))
                {
                    LogAudit("Register", "Users", $"Failed Registeration attempt because Email already exist", 0);
                    return RedirectToAction("Register", "Auth", new { msg = "Email already exist, Try another!", color = "red" });
                }

                user.Password = StringCipher.Encrypt(user.Password.Trim());
                user.SysCreatedDate = DateTime.Now;
                user.IsActive = true;
                user.IsAdmin = false;

                bool success = new UserBL().AddUser(user, db);

                if (success)
                {
                    LogAudit("Register","Users", "New user registered", user.PK_UserId);
                    return RedirectToAction("Login", "Auth", new { msg = "Registration successful! Please login.", color = "green" });
                }
                LogAudit("Register", "Users", "Something went wrong with user registeration", 0);
                return RedirectToAction("Register", "Auth", new { msg = "Something went wrong", color = "red" });
            }
            catch
            {
                LogAudit("Register", "Users", "Unexpected error occurred with user registeration", 0);
                return RedirectToAction("Register", "Auth", new { msg = "Unexpected error occurred", color = "red" });
            }
        }
        #endregion

        #region ForgotPassword
        [AllowAnonymous]
        public ActionResult ForgotPassword(string msg = "", string color = "black")
        {
            ViewBag.Message = msg;
            ViewBag.Color = color;
            return View();
        }

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
                    LogAudit("Forgot Password","Users", "Reset password link sent", user.PK_UserId);
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
                    LogAudit("Reset Password","User", "Password reset successful", user.PK_UserId);
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
                user.SysModifiedDate = DateTime.Now;

                bool chkUser = new UserBL().UpdateUser(user, db);
                if (chkUser == true)
                {
                    LogAudit("Update Profile","Users", "Profile updated successfully", user.PK_UserId);
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
            var user = gp.ValidateLoggedinUser();
            if (user != null)
            {
                LogAudit("Logout","User", $"User logged out: {user.Email}", user.PK_UserId);
            }
            return RedirectToAction("Login");
        }
        #endregion
    }

    public class ChallengeResult : HttpUnauthorizedResult
    {
        public ChallengeResult(string provider, string redirectUri)
            : this(provider, redirectUri, null) { }
        public ChallengeResult(string provider, string redirectUri, string userId)
        {
            LoginProvider = provider;
            RedirectUri = redirectUri;
            UserId = userId;
        }
        public string LoginProvider { get; set; }
        public string RedirectUri { get; set; }
        public string UserId { get; set; }
        public override void ExecuteResult(ControllerContext context)
        {
            var properties = new AuthenticationProperties
            {
                RedirectUri = RedirectUri,
                Dictionary = {
                    ["LoginProvider"] = LoginProvider,
                    ["RedirectUri"] = RedirectUri
                }
            };
            if (UserId != null) properties.Dictionary["XsrfId"] = UserId;
            context.HttpContext.GetOwinContext().Authentication.Challenge(properties, LoginProvider);
        }
    }
}