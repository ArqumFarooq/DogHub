using DogHub.BL;
using DogHub.Models;
using System.Linq;
using System.Web.Mvc;

namespace DogHub.Controllers
{
    public class AdminController : Controller
    {
        private DogHubEntities db = new DogHubEntities();

        [Authorize(Roles = "Admin")]
        public ActionResult Dashboard()
        {
            var totalUsers = db.Users.Count(u => (u.IsActive != false || u.IsActive != null) && u.IsAdmin != true);
            var totalBreeds = db.DogBreeds.Count(b => b.IsDeleted == false || b.IsDeleted == null);

            ViewBag.TotalUsers = totalUsers;
            ViewBag.TotalDogBreeds = totalBreeds;

            return View();
        }

        public ActionResult Users()
        {
            var users = db.Users
                .Where(u => u.IsActive == false || u.IsActive == null)
                .ToList();

            return View(users);
        }

        public ActionResult AuditLogs()
        {
            var logs = db.AuditLogs
                .OrderByDescending(x => x.SysCreatedDate)
                .Take(100)
                .ToList();

            return View(logs);
        }

        public ActionResult DogBreeds()
        {
            return RedirectToAction("Index", "DogBreed");
        }
    }
}
