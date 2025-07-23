using DogHub.BL;
using DogHub.HelpingClasses;
using DogHub.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace DogHub.Controllers
{
    public class HomeController : Controller
    {
        private readonly DogHubEntities db = new DogHubEntities();

        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Detail(int AuthorId = 1, int Heart = 1, int DogId = 1)
        {
            DogBreed u = new DogBreedBL().GetDogBreedById(DogId, db);

            ViewBag.PK_DogBreedId = u.PK_DogBreedId;
            ViewBag.DogName = u.DogName;
            ViewBag.ParentBreedId = u.ParentBreedId;
            ViewBag.ParentBreed = u.ParentBreed;
            ViewBag.Origin = u.Origin;
            ViewBag.LifeSpan = u.LifeSpan;
            ViewBag.Description = u.Description;
            ViewBag.ImageUrl = u.ImageUrl;
            ViewBag.AuthorImage = $"/Content/Home/images/author/author-{AuthorId}.jpg";
            ViewBag.AuthorId = AuthorId;
            ViewBag.Heart = Heart;
            ViewBag.dogsubBreed = new DogBreedBL().GetAllDogBreeds(db).Where(x => (x.IsDeleted == false || x.IsDeleted == null)  && (x.ParentBreedId == DogId)).ToList();
            ViewBag.dogsubBreedCount = new DogBreedBL().GetAllDogBreeds(db).Where(x => (x.IsDeleted == false || x.IsDeleted == null) && (x.ParentBreedId == DogId)).Count();

            return View();
        }

        public ActionResult SearchDog()
        {
            var dogBreedList = db.DogBreeds.Take(8).ToList(); // Initial load
            return View(dogBreedList);
        }

        public ActionResult SearchDogPartial(string search, int skip = 0)
        {
            int take = 8;

            // Use the BL to get the complete list and apply the necessary filters
            var dogBreedList = new DogBreedBL().GetAllDogBreeds(db)
                .Where(x => (x.IsDeleted == false || x.IsDeleted == null) && (x.ImageUrl != null && x.DogName != null && x.Origin != null && x.LifeSpan != null)).ToList();

            // Apply search filter if provided
            if (!string.IsNullOrEmpty(search))
                dogBreedList = dogBreedList.Where(x => x.DogName.Contains(search)).ToList();

            // Order, paginate, and convert to list
            var list = dogBreedList.OrderBy(x => x.DogName).ToList();

            // Generate DTOs
            var rand = new Random();
            var dtoList = list.Select((x, index) => new DogBreedDTO
            {
                DogName = x.DogName,
                Origin = x.Origin,
                LifeSpan = x.LifeSpan,
                ImageUrl = x.ImageUrl,
                Hearts = rand.Next(1, 100),
                AuthorImage = $"/Content/Home/images/author/author-{(skip + index + 1)}.jpg"
            }).ToList();

            // Render partial view
            var html = RenderPartialViewToString("_DogBreedCardPartial", dtoList);

            return Json(new
            {
                html = html,
                loadedCount = skip + dtoList.Count
            }, JsonRequestBehavior.AllowGet);
        }

        protected string RenderPartialViewToString(string viewName, object model)
        {
            if (string.IsNullOrEmpty(viewName))
                viewName = ControllerContext.RouteData.GetRequiredString("action");

            ViewData.Model = model;

            using (var sw = new StringWriter())
            {
                var viewResult = ViewEngines.Engines.FindPartialView(ControllerContext, viewName);
                var viewContext = new ViewContext(ControllerContext, viewResult.View, ViewData, TempData, sw);
                viewResult.View.Render(viewContext, sw);
                viewResult.ViewEngine.ReleaseView(ControllerContext, viewResult.View);
                return sw.GetStringBuilder().ToString();
            }
        }

    }
}