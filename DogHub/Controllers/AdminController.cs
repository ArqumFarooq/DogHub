using DogHub.BL;
using DogHub.HelpingClasses;
using DogHub.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using HttpPostAttribute = System.Web.Mvc.HttpPostAttribute;

namespace DogHub.Controllers
{
    public class AdminController : Controller
    {
        private readonly GeneralPurpose gp = new GeneralPurpose();
        private readonly DogHubEntities de = new DogHubEntities();

        private void LogAudit(string actionType, string affectedTable, string details, int? userId = null)
        {
            var auditBL = new AuditLogBL();
            auditBL.AddAuditLog(
                actionType: actionType,
                affectedTable: affectedTable,
                auditDetails: details,
                createdById: userId,
                de: de
            );
        }

        #region ValidateLoginUser
        public bool ValidateLogin()
        {
            if (gp.ValidateLoggedinUser() != null)
            {
                if (gp.ValidateLoggedinUser().IsAdmin == true || gp.ValidateLoggedinUser().IsAdmin == false)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                return false;
            }
        }
        public bool ValidateAdminLogin()
        {
            if (gp.ValidateLoggedinUser() != null)
            {
                if (gp.ValidateLoggedinUser().IsAdmin == true)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                return false;
            }
        }
        public bool ValidateUserLogin()
        {
            if (gp.ValidateLoggedinUser() != null)
            {
                if (gp.ValidateLoggedinUser().IsAdmin == false)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                return false;
            }
        }
        #endregion

        #region Dashboard
        public ActionResult Dashboard(string msg = "", string color = "black")
        {
            if (ValidateLogin() == false)
            {
                return RedirectToAction("Login", "Auth", new { msg = "Session Expired! Please Login", color = "red" });
            }

            var identity = (System.Security.Claims.ClaimsPrincipal)System.Threading.Thread.CurrentPrincipal;
            var id = identity.Claims.Where(c => c.Type == System.Security.Claims.ClaimTypes.Sid).Select(c => c.Value).SingleOrDefault();
            User loggedinUser = new UserBL().GetUserById(Convert.ToInt32(id), de);

            if (loggedinUser.IsAdmin == true)
            {
                ViewBag.UserCount = new UserBL().GetActiveUsers(de).Where(x => x.IsAdmin != true && (x.IsActive != false || x.IsActive != null)).Count();
                ViewBag.DogBreedCount = new DogBreedBL().GetAllDogBreeds(de).Where(x => x.IsDeleted == false || x.IsDeleted == null).Count();
                ViewBag.DogBreedCountByUser = new DogBreedBL().GetAllDogBreeds(de).Where(x => (x.IsDeleted == false || x.IsDeleted == null) && x.SysCreatedID == loggedinUser.PK_UserId).Count();
                ViewBag.AuditLogCount = new AuditLogBL().GetAllAuditLogs(de).Count();
                ViewBag.AuditLogCountByUser = new AuditLogBL().GetAllAuditLogs(de).Where(x => x.SysCreatedID == loggedinUser.PK_UserId).Count();
                ViewBag.Message = msg;
                ViewBag.Color = color;

                return View();
            }
            else
            {
                ViewBag.DogBreedCount = new DogBreedBL().GetAllDogBreeds(de).Where(x => x.IsDeleted == false || x.IsDeleted == null).Count();
                ViewBag.DogBreedCountByUser = new DogBreedBL().GetAllDogBreeds(de).Where(x => (x.IsDeleted == false || x.IsDeleted == null) && x.SysCreatedID == loggedinUser.PK_UserId).Count();
                ViewBag.Message = msg;
                ViewBag.Color = color;

                return View();
            }
        }
        #endregion

        #region Manage User 
        public ActionResult AddUser(string msg = "", string color = "black")
        {
            if (ValidateAdminLogin() == false)
            {
                return RedirectToAction("Login", "Auth", new { msg = "Session Expired! Please Login", color = "red" });
            }

            ViewBag.Message = msg;
            ViewBag.Color = color;

            return View();
        }

        [HttpPost]
        public ActionResult PostAddUser(User _user)
        {
            try
            {
                if (ValidateAdminLogin() == false)
                {
                    return RedirectToAction("Login", "Auth", new { msg = "Session Expired! Please Login", color = "red" });
                }

                bool checkEmail = gp.ValidateEmail(_user.Email);
                if (checkEmail == false)
                {
                    return RedirectToAction("AddUser", "Admin", new { msg = "Email already exist. Try another!", color = "red" });
                }

                User obj = new User()
                {
                    UserName = _user.UserName.Trim(),
                    Email = _user.Email.Trim(),
                    Password = StringCipher.Encrypt(_user.Password.Trim()),
                    IsAdmin = false,
                    IsActive = true,
                    AuthId = null,
                    AuthProvider = null,
                    SysCreatedDate = DateTime.Now
                };

                bool checkUser = new UserBL().AddUser(obj, de);
                if (checkUser == true)
                {
                    LogAudit("AddUser", "Users", $"New user added by admin: {obj.Email}", 1);
                    return RedirectToAction("AddUser", "Admin", new { msg = "Record inserted successfully", color = "green" });
                }
                else
                {
                    return RedirectToAction("AddUser", "Admin", new { msg = "Somethings' Wrong", color = "red" });
                }
            }
            catch
            {
                return RedirectToAction("Index", "Error");
            }
        }

        public ActionResult ViewUser(string msg = "", string color = "black")
        {
            if (ValidateAdminLogin() == false)
            {
                return RedirectToAction("Login", "Auth", new { msg = "Session Expired! Please Login", color = "red" });
            }

            ViewBag.Message = msg;
            ViewBag.Color = color;

            return View();
        }

        [HttpPost]
        public ActionResult PostUpdateUser(User _user)
        {
            try
            {
                if (ValidateAdminLogin() == false)
                {
                    return RedirectToAction("Login", "Auth", new { msg = "Session Expired! Please Login", color = "red" });
                }

                bool checkEmail = gp.ValidateEmail(_user.Email, _user.PK_UserId);
                if (checkEmail == false)
                {
                    return RedirectToAction("ViewUser", "Admin", new { msg = "Email already exist. Try another!", color = "red" });
                }

                User u = new UserBL().GetUserById(_user.PK_UserId, de);
                u.UserName = _user.UserName.Trim();
                u.Email = _user.Email.Trim();

                bool checkUser = new UserBL().UpdateUser(u, de);
                if (checkUser == true)
                {
                    LogAudit("UpdateUser", "Users", $"User Record updated successfully by admin: {u.Email}", 1);
                    return RedirectToAction("ViewUser", "Admin", new { msg = "Record updated successfully", color = "green" });
                }
                else
                {
                    return RedirectToAction("ViewUser", "Admin", new { msg = "Somethings' Wrong", color = "red" });
                }
            }
            catch
            {
                return RedirectToAction("Index", "Error");
            }
        }

        public ActionResult DeleteUser(int id)
        {
            try
            {
                if (ValidateAdminLogin() == false)
                {
                    return RedirectToAction("Login", "Auth", new { msg = "Session Expired! Please Login", color = "red" });
                }
                User u = new UserBL().GetUserById(id, de);
                u.IsActive = false;

                bool checkUser = new UserBL().UpdateUser(u, de);
                if (checkUser == true)
                {
                    LogAudit("DeleteUser", "Users", $"User Record Deleted successfully by admin: {u.Email}", 1);
                    return RedirectToAction("ViewUser", "Admin", new { msg = "Record Deleted successfully", color = "green" });
                }
                else
                {
                    return RedirectToAction("ViewUser", "Admin", new { msg = "Somethings' Wrong", color = "red" });
                }
            }
            catch
            {
                return RedirectToAction("Index", "Error");
            }
        }

        [HttpPost]
        public ActionResult GetUserList(string UserName = "", string Email = "")
        {
            List<User> ulist = new UserBL().GetActiveUsers(de).Where(x => x.IsAdmin != true).OrderByDescending(x => x.PK_UserId).ToList();

            if (UserName != "")
            {
                ulist = ulist.Where(x => x.UserName.ToLower().Contains(UserName.ToLower())).ToList();
            }
            if (Email != "")
            {
                ulist = ulist.Where(x => x.Email.ToLower().Contains(Email.ToLower())).ToList();
            }

            int start = Convert.ToInt32(Request["start"]);
            int length = Convert.ToInt32(Request["length"]);
            string searchValue = Request["search[value]"];
            string sortColumnName = Request["columns[" + Request["order[0][column]"] + "][name]"];
            string sortDirection = Request["order[0][dir]"];
            if (sortColumnName != "" && sortColumnName != null)
            {
                if (sortColumnName != "0")
                {
                    if (sortDirection == "asc")
                    {
                        ulist = ulist.OrderBy(x => x.GetType().GetProperty(sortColumnName).GetValue(x)).ToList();
                    }
                    else
                    {
                        ulist = ulist.OrderByDescending(x => x.GetType().GetProperty(sortColumnName).GetValue(x)).ToList();
                    }
                }
            }
            int totalrows = ulist.Count();

            //filter
            if (searchValue != "")
            {
                ulist = ulist.Where(x => x.UserName.ToLower().Contains(searchValue.ToLower()) || x.Email.ToLower().Contains(searchValue.ToLower())).ToList();
            }

            int totalrowsafterfilterinig = ulist.Count();

            //pagination
            ulist = ulist.Skip(start).Take(length).ToList();

            List<UserDTO> udto = new List<UserDTO>();
            foreach (User u in ulist)
            {
                UserDTO obj = new UserDTO()
                {
                    PK_UserId = u.PK_UserId,
                    UserName = u.UserName,
                    Email = u.Email,
                };

                udto.Add(obj);
            }
            return Json(new { data = udto, draw = Request["draw"], recordsTotal = totalrows, recordsFiltered = totalrowsafterfilterinig }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetUserById(int id)
        {
            User u = new UserBL().GetUserById(id, de);
            User obj = new User()
            {
                PK_UserId = u.PK_UserId,
                UserName = u.UserName,
                Email = u.Email
            };

            return Json(obj, JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region Manage Dog Breed 
        public ActionResult AddDogBreed(string msg = "", string color = "black")
        {
            if (ValidateLogin() == false)
            {
                return RedirectToAction("Login", "Auth", new { msg = "Session Expired! Please Login", color = "red" });
            }

            ViewBag.Message = msg;
            ViewBag.Color = color;
            ViewBag.ParentBreeds = new DogBreedBL().GetAllDogBreeds(de);
            ViewBag.Countries = gp.GetAllCountries();
            return View();
        }

        [HttpPost]
        public ActionResult PostAddDogBreed(DogBreed _dogBreed, HttpPostedFileBase ImageFile, string DogImageUrl)
        {
            try
            {
                var identity = (System.Security.Claims.ClaimsPrincipal)System.Threading.Thread.CurrentPrincipal;
                var id = identity.Claims.Where(c => c.Type == System.Security.Claims.ClaimTypes.Sid).Select(c => c.Value).SingleOrDefault();

                User loggedinUser = new UserBL().GetUserById(Convert.ToInt32(id), de);

                if (ValidateLogin() == false)
                {
                    return RedirectToAction("Login", "Auth", new { msg = "Session Expired! Please Login", color = "red" });
                }

                bool check = gp.ISDogBreedExist(_dogBreed.DogName);
                if (check == false)
                {
                    return RedirectToAction("AddDogBreed", "Admin", new { msg = "Dog Breed already exists. Try another!", color = "red" });
                }

                bool chk = gp.ISDogNameANDParentSame(_dogBreed.DogName, _dogBreed.ParentBreedId);
                if (chk)
                {
                    return RedirectToAction("AddDogBreed", "Admin", new { msg = "Dog Breed Name & Parent Name can't be same.", color = "red" });
                }

                if (ImageFile != null && ImageFile.ContentLength > 0)
                {
                    // Optional: check file size
                    if (ImageFile.ContentLength > 10 * 1024 * 1024) // 10MB
                    {
                        return RedirectToAction("AddDogBreed", "Admin", new { msg = "Image size is exceeding the limits", color = "red" });
                    }

                    // validate MIME type
                    string[] permittedTypes = { "image/jpeg", "image/png", "image/jpg", "image/gif", "image/webp" };
                    if (!permittedTypes.Contains(ImageFile.ContentType.ToLower()))
                    {
                        return RedirectToAction("AddDogBreed", "Admin", new { msg = "Only image files (JPG, PNG, GIF, WEBP) are allowed.", color = "red" });
                    }

                    // validating extension (double security layer)
                    string[] allowedExtensions = { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
                    string fileExt = Path.GetExtension(ImageFile.FileName).ToLower();

                    if (!allowedExtensions.Contains(fileExt))
                    {
                        return RedirectToAction("AddDogBreed", "Admin", new { msg = "Invalid file extension. Please upload image files only.", color = "red" });
                    }

                    string fileName = Path.GetFileName(ImageFile.FileName);
                    string uploadPath = Server.MapPath("~/Uploads/DogImages/");
                    if (!Directory.Exists(uploadPath))
                    {
                        Directory.CreateDirectory(uploadPath);
                    }

                    string filePath = Path.Combine("/Uploads/DogImages/", fileName);
                    string fullPath = Path.Combine(uploadPath, fileName);

                    ImageFile.SaveAs(fullPath);
                    _dogBreed.ImageUrl = filePath;
                }

                if (!string.IsNullOrEmpty(DogImageUrl))
                {
                    _dogBreed.ImageUrl = DogImageUrl;
                }

                DogBreed obj = new DogBreed()
                {
                    DogName = _dogBreed.DogName,
                    ParentBreedId = _dogBreed.ParentBreedId,
                    Origin = _dogBreed.Origin,
                    LifeSpan = _dogBreed.LifeSpan,
                    Description = _dogBreed.Description,
                    ImageUrl = _dogBreed.ImageUrl,
                    IsDeleted = false,
                    SysCreatedID = loggedinUser.PK_UserId, // current user Id
                    SysCreatedDate = DateTime.Now
                };

                bool checkDogbreed = new DogBreedBL().AddDogBreed(obj, de);
                if (checkDogbreed == true)
                {
                    LogAudit("AddDogBreed", "DogBreeds", $"DogBreed Record Added Successfully by admin: {obj.DogName}", loggedinUser.PK_UserId);
                    return RedirectToAction("AddDogBreed", "Admin", new { msg = "Record inserted successfully", color = "green" });
                }
                else
                {
                    return RedirectToAction("AddDogBreed", "Admin", new { msg = "Something went wrong", color = "red" });
                }
            }
            catch
            {
                return RedirectToAction("Index", "Error");
            }
        }

        public ActionResult ViewDogBreed(string msg = "", string color = "black")
        {
            if (ValidateLogin() == false)
            {
                return RedirectToAction("Login", "Auth", new { msg = "Session Expired! Please Login", color = "red" });
            }

            ViewBag.Message = msg;
            ViewBag.Color = color;
            ViewBag.ParentBreeds = new DogBreedBL().GetAllDogBreeds(de); // Or filtered list
            ViewBag.Countries = new List<string> { "Pakistan", "USA", "UK", "Germany", "Japan" }; // or fetch from DB

            return View();
        }

        [HttpPost]
        public ActionResult PostUpdateDogBreed(DogBreed _dogBreed, HttpPostedFileBase ImageFile, string DogImageUrl)
        {
            try
            {
                if (ValidateAdminLogin() == false)
                {
                    return RedirectToAction("Login", "Auth", new { msg = "Session Expired! Please Login", color = "red" });
                }

                bool check = gp.ISDogBreedExist(_dogBreed.DogName);
                if (check == false)
                {
                    return RedirectToAction("ViewDogBreed", "Admin", new { msg = "Dog Breed already exists. Try another!", color = "red" });
                }
                bool chk = gp.ISDogNameANDParentSame(_dogBreed.DogName, _dogBreed.ParentBreedId);
                if (chk)
                {
                    return RedirectToAction("AddDogBreed", "Admin", new { msg = "Dog Breed Name & Parent Name can't be same.", color = "red" });
                }

                DogBreed u = new DogBreedBL().GetBreedById(_dogBreed.PK_DogBreedId, de);
                u.DogName = _dogBreed.DogName.Trim();
                u.ParentBreedId = _dogBreed.ParentBreedId;
                u.Origin = _dogBreed.Origin.Trim();
                u.LifeSpan = _dogBreed.LifeSpan.Trim();
                u.Description = _dogBreed.Description.Trim();
                u.SysCreatedID = 1;
                u.SysModifiedDate = DateTime.Now;

                if (ImageFile != null && ImageFile.ContentLength > 0)
                {
                    string[] permittedTypes = { "image/jpeg", "image/png", "image/jpg", "image/gif", "image/webp" };
                    if (!permittedTypes.Contains(ImageFile.ContentType.ToLower()))
                    {
                        return RedirectToAction("ViewDogBreed", "Admin", new { msg = "Only image files (JPG, PNG, GIF, WEBP) are allowed.", color = "red" });
                    }

                    string[] allowedExtensions = { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
                    string fileExt = Path.GetExtension(ImageFile.FileName).ToLower();
                    if (!allowedExtensions.Contains(fileExt))
                    {
                        return RedirectToAction("ViewDogBreed", "Admin", new { msg = "Invalid file extension. Please upload image files only.", color = "red" });
                    }

                    string fileName = Path.GetFileName(ImageFile.FileName);
                    string uploadPath = Server.MapPath("~/Uploads/DogImages/");
                    if (!Directory.Exists(uploadPath))
                    {
                        Directory.CreateDirectory(uploadPath);
                    }

                    string filePath = Path.Combine("/Uploads/DogImages/", fileName);
                    string fullPath = Path.Combine(uploadPath, fileName);

                    ImageFile.SaveAs(fullPath);
                    u.ImageUrl = filePath;
                }

                if (!string.IsNullOrEmpty(DogImageUrl))
                {
                    u.ImageUrl = DogImageUrl;
                }

                bool checkdog = new DogBreedBL().UpdateDogBreed(u, de);
                if (checkdog == true)
                {
                    LogAudit("UpdatedDogBreed", "DogBreeds", $"DogBreed Record Updated Successfully by admin: {u.DogName}", 1);
                    return RedirectToAction("ViewDogBreed", "Admin", new { msg = "Record updated successfully", color = "green" });
                }
                else
                {
                    return RedirectToAction("ViewDogBreed", "Admin", new { msg = "Something went wrong", color = "red" });
                }
            }
            catch
            {
                return RedirectToAction("Index", "Error");
            }
        }

        public ActionResult DeleteDogBreed(int id)
        {
            try
            {
                if (ValidateAdminLogin() == false)
                {
                    return RedirectToAction("Login", "Auth", new { msg = "Session Expired! Please Login", color = "red" });
                }
                DogBreed u = new DogBreedBL().GetBreedById(id, de);
                u.IsDeleted = true;

                bool checkUser = new DogBreedBL().DeleteDogBreed(u.PK_DogBreedId, de);
                if (checkUser == true)
                {
                    LogAudit("DeletedDogBreed", "DogBreeds", $"DogBreed Record Deleted Successfully by admin: {u.DogName}", 1);
                    return RedirectToAction("ViewUser", "Admin", new { msg = "Record Deleted successfully", color = "green" });
                }
                else
                {
                    return RedirectToAction("ViewUser", "Admin", new { msg = "Somethings' Wrong", color = "red" });
                }
            }
            catch
            {
                return RedirectToAction("Index", "Error");
            }
        }

        [HttpPost]
        public ActionResult GetDogBreedList(string DogName = "", string Origin = "", string LifeSpan = "")
        {
            List<DogBreed> dogList = new DogBreedBL().GetAllDogBreeds(de).OrderByDescending(x => x.PK_DogBreedId).ToList();

            // Filtering
            if (!string.IsNullOrEmpty(DogName))
            {
                dogList = dogList.Where(x => x.DogName != null && x.DogName.ToLower().Contains(DogName.ToLower())).ToList();
            }
            if (!string.IsNullOrEmpty(Origin))
            {
                dogList = dogList.Where(x => x.Origin != null && x.Origin.ToLower().Contains(Origin.ToLower())).ToList();
            }
            if (!string.IsNullOrEmpty(LifeSpan))
            {
                dogList = dogList.Where(x => x.LifeSpan != null && x.LifeSpan.ToLower().Contains(LifeSpan.ToLower())).ToList();
            }

            int start = Convert.ToInt32(Request["start"]);
            int length = Convert.ToInt32(Request["length"]);
            string searchValue = Request["search[value]"];
            string sortColumnName = Request["columns[" + Request["order[0][column]"] + "][name]"];
            string sortDirection = Request["order[0][dir]"];

            int totalrows = dogList.Count();

            // Global Search
            if (!string.IsNullOrEmpty(searchValue))
            {
                searchValue = searchValue.ToLower();
                dogList = dogList.Where(x =>
                    (x.DogName != null && x.DogName.ToLower().Contains(searchValue)) ||
                    (x.Origin != null && x.Origin.ToLower().Contains(searchValue)) ||
                    (x.LifeSpan != null && x.LifeSpan.ToLower().Contains(searchValue)) ||
                    (x.Description != null && x.Description.ToLower().Contains(searchValue))
                ).ToList();
            }

            int totalrowsafterfilterinig = dogList.Count();

            // Sorting
            if (!string.IsNullOrEmpty(sortColumnName))
            {
                var propInfo = typeof(DogBreed).GetProperty(sortColumnName);
                if (propInfo != null)
                {
                    dogList = sortDirection == "asc"
                        ? dogList.OrderBy(x => propInfo.GetValue(x)).ToList()
                        : dogList.OrderByDescending(x => propInfo.GetValue(x)).ToList();
                }
            }

            // Paging
            dogList = dogList.Skip(start).Take(length).ToList();

            List<DogBreedDTO> dto = new List<DogBreedDTO>();
            foreach (DogBreed d in dogList)
            {
                DogBreedDTO obj = new DogBreedDTO()
                {
                    PK_DogBreedId = d.PK_DogBreedId,
                    DogName = d.DogName,
                    ParentBreedId = d.ParentBreedId,
                    Origin = d.Origin,
                    LifeSpan = d.LifeSpan,
                    Description = d.Description,
                    ImageUrl = d.ImageUrl,
                    SubBreedCount = d.SubBreeds.Count
                };

                dto.Add(obj);
            }

            return Json(new { data = dto, draw = Request["draw"], recordsTotal = totalrows, recordsFiltered = totalrowsafterfilterinig }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetDogBreedById(int id)
        {
            DogBreed u = new DogBreedBL().GetDogBreedById(id, de);

            var obj = new
            {
                PK_DogBreedId = u.PK_DogBreedId,
                DogName = u.DogName,
                ParentBreedId = u.ParentBreedId,
                Origin = u.Origin,
                LifeSpan = u.LifeSpan,
                Description = u.Description,
                ImageUrl = u.ImageUrl
            };

            return Json(obj, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult GenerateBreedDescription(string DogName = "", string ParentBreed = "", string Origin = "", string LifeSpan = "")
        {
            #region Get description from chatgpt
            //string prompt = $"Write a unique, friendly dog breed description for a breed named '{data.DogName}', originated from '{data.Origin}', derived from '{data.ParentBreed}', with an average lifespan of {data.LifeSpan} years.";

            //string apiKey = "xyz";

            //using (var client = new HttpClient())
            //{
            //    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);

            //    var requestData = new
            //    {
            //        model = "gpt-3.5-turbo",
            //        messages = new[]
            //        {
            //            new { role = "user", content = prompt }
            //        }
            //    };

            //    var content = new StringContent(JsonConvert.SerializeObject(requestData), Encoding.UTF8, "application/json");

            //    var response = await client.PostAsync("https://api.openai.com/v1/chat/completions", content);
            //    var json = await response.Content.ReadAsStringAsync();
            //    dynamic result = JsonConvert.DeserializeObject(json);

            //    string aiDescription = result.choices[0].message.content;

            //    return Json(new { description = aiDescription });
            #endregion

            string description = $"The '{DogName}' is a remarkable breed that traces its roots back to '{Origin}'. ";

            if (!string.IsNullOrWhiteSpace(ParentBreed) && ParentBreed != "-- Select Parent Breed (optional)--")
            {
                description += $"With lineage derived from the noble '{ParentBreed}', this breed carries both grace and strength. ";
            }
            else
            {
                description += $"It has developed a unique identity through generations of natural traits and breeding. ";
            }

            description += $"Known for its loyalty and playful nature, the '{DogName}' is a perfect companion for families and individuals alike. " +
                           $"On average, it enjoys a lifespan of around '{LifeSpan}' years, bringing years of joy and unforgettable memories.";

            return Json(new { description });
        }

        #endregion

        #region Manage Audit Log

        public ActionResult ViewAuditLog(string msg = "", string color = "black")
        {
            if (ValidateAdminLogin() == false)
            {
                return RedirectToAction("Login", "Auth", new { msg = "Session Expired! Please Login", color = "red" });
            }

            ViewBag.Message = msg;
            ViewBag.Color = color;

            return View();
        }

        [HttpPost]
        public ActionResult GetAuditLogList(string ActionType = "")
        {
            List<AuditLog> auditLoglist = new AuditLogBL().GetAllAuditLogs(de).OrderByDescending(x => x.PK_AuditLogId).ToList();

            if (ActionType != "")
            {
                auditLoglist = auditLoglist.Where(x => x.ActionType.ToLower().Contains(ActionType.ToLower())).ToList();
            }

            int start = Convert.ToInt32(Request["start"]);
            int length = Convert.ToInt32(Request["length"]);
            string searchValue = Request["search[value]"];
            string sortColumnName = Request["columns[" + Request["order[0][column]"] + "][name]"];
            string sortDirection = Request["order[0][dir]"];
            if (sortColumnName != "" && sortColumnName != null)
            {
                if (sortColumnName != "0")
                {
                    if (sortDirection == "asc")
                    {
                        auditLoglist = auditLoglist.OrderBy(x => x.GetType().GetProperty(sortColumnName).GetValue(x)).ToList();
                    }
                    else
                    {
                        auditLoglist = auditLoglist.OrderByDescending(x => x.GetType().GetProperty(sortColumnName).GetValue(x)).ToList();
                    }
                }
            }
            int totalrows = auditLoglist.Count();

            //filter
            if (searchValue != "")
            {
                auditLoglist = auditLoglist.Where(x => x.ActionType.ToLower().Contains(searchValue.ToLower())).ToList();
            }

            int totalrowsafterfilterinig = auditLoglist.Count();

            //pagination
            auditLoglist = auditLoglist.Skip(start).Take(length).ToList();

            List<AuditLogDTO> udto = new List<AuditLogDTO>();
            foreach (AuditLog a in auditLoglist)
            {
                AuditLogDTO obj = new AuditLogDTO()
                {
                    PK_AuditLogId = a.PK_AuditLogId,
                    ActionType = a.ActionType,
                    AffectedTable = a.AffectedTable,
                    AuditDetails = a.AuditDetails
                };

                udto.Add(obj);
            }
            return Json(new { data = udto, draw = Request["draw"], recordsTotal = totalrows, recordsFiltered = totalrowsafterfilterinig }, JsonRequestBehavior.AllowGet);
        }

        #endregion
    }
}
