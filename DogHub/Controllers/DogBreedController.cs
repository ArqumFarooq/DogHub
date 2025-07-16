using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using DogHub.Models;
using DogHub.BL;
using DogHub.HelpingClasses;

namespace DogHub.Controllers
{
    public class DogBreedController : Controller
    {
        private readonly DogHubEntities db = new DogHubEntities();
        private readonly DogBreedBL dogBreedBL = new DogBreedBL();
        private readonly AuditLogBL auditBL = new AuditLogBL();
        private readonly GeneralPurpose gp = new GeneralPurpose();

        // GET: DogBreed

        [Authorize]
        public ActionResult Index()
        {
            var dogBreeds = dogBreedBL.GetAllDogBreeds(db);
            return View(dogBreeds);
        }

        // GET: DogBreed/Details/5
        [Authorize]
        public ActionResult Details(int? id)
        {
            if (id == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            var dogBreed = dogBreedBL.GetDogBreedById(id.Value, db);
            if (dogBreed == null)
                return HttpNotFound();

            return View(dogBreed);
        }

        // GET: DogBreed/Create
        [Authorize]
        public ActionResult Create()
        {
            var parentBreedSelectList = new List<SelectListItem>
            {
                new SelectListItem { Text = "-- Choose Parent Breed (optional) --", Value = null }
            };

            parentBreedSelectList.AddRange(db.DogBreeds
                .Where(x => x.ParentBreedId == null && (x.IsDeleted == false || x.IsDeleted == null))
                .Select(x => new SelectListItem
                {
                    Text = x.DogName,
                    Value = x.PK_DogBreedId.ToString()
                }));

            ViewBag.ParentBreedId = parentBreedSelectList;

            //ViewBag.ParentBreedId = new SelectList(dogBreedBL.GetAllDogBreeds(db), "PK_DogBreedId", "DogName");
            return View();
        }

        // POST: DogBreed/Create
        [HttpPost]
        [Authorize]
        //[ValidateAntiForgeryToken]
        public ActionResult Create(DogBreed dogBreed)
        {
            if (ModelState.IsValid)
            {
                dogBreed.SysCreatedDate = DateTime.Now;
                dogBreed.SysCreatedID = gp.ValidateLoggedinUser()?.PK_UserId ?? 0;

                bool success = dogBreedBL.AddDogBreed(dogBreed, db);
                if (success)
                {
                    auditBL.AddAuditLog("CREATE", "DogBreeds", $"Created DogBreed: {dogBreed.DogName}", dogBreed.SysCreatedID.Value, db);
                    return RedirectToAction("Index");
                }
            }

            ViewBag.ParentBreedId = new SelectList(dogBreedBL.GetAllDogBreeds(db), "PK_DogBreedId", "DogName", dogBreed.ParentBreedId);
            return View(dogBreed);
        }

        // GET: DogBreed/Edit/5
        [Authorize(Roles = "Admin")]
        public ActionResult Edit(int? id)
        {
            if (id == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            DogBreed dogBreed = dogBreedBL.GetDogBreedById(id.Value, db);
            if (dogBreed == null)
                return HttpNotFound();

            ViewBag.ParentBreedId = new SelectList(dogBreedBL.GetAllDogBreeds(db), "PK_DogBreedId", "DogName", dogBreed.ParentBreedId);
            return View(dogBreed);
        }

        // POST: DogBreed/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public ActionResult Edit(DogBreed dogBreed)
        {
            if (ModelState.IsValid)
            {
                dogBreed.SysModifiedDate = DateTime.Now;
                dogBreed.SysModifiedID = gp.ValidateLoggedinUser()?.PK_UserId ?? 0;

                bool updated = dogBreedBL.UpdateDogBreed(dogBreed, db);
                if (updated)
                {
                    auditBL.AddAuditLog("UPDATE", "DogBreeds", $"Updated DogBreed: {dogBreed.DogName}", dogBreed.SysModifiedID.Value, db);
                    return RedirectToAction("Index");
                }
            }

            ViewBag.ParentBreedId = new SelectList(dogBreedBL.GetAllDogBreeds(db), "PK_DogBreedId", "DogName", dogBreed.ParentBreedId);
            return View(dogBreed);
        }

        // GET: DogBreed/Delete/5
        [Authorize(Roles = "Admin")]
        public ActionResult Delete(int? id)
        {
            if (id == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            DogBreed dogBreed = dogBreedBL.GetDogBreedById(id.Value, db);
            if (dogBreed == null)
                return HttpNotFound();

            return View(dogBreed);
        }

        //public ActionResult DeleteConfirmed(int id)
        //{
        //    DogBreed dogBreed = dogBreedBL.GetDogBreedById(id, db);
        //    if (dogBreed != null)
        //    {
        //        bool deleted = dogBreedBL.DeleteDogBreed(dogBreed.PK_DogBreedId, db);
        //        if (deleted)
        //        {
        //            int userId = gp.ValidateLoggedinUser()?.PK_UserId ?? 0;
        //            auditBL.AddAuditLog("DELETE", "DogBreeds", $"Deleted DogBreed: {dogBreed.DogName}", userId, db);
        //        }
        //    }

        //    return RedirectToAction("Index");
        //}
        // POST: DogBreed/Delete/5
        //[HttpPost, ActionName("Delete")]
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public ActionResult DeleteConfirmed(int id)
        {
            var db = new DogHubEntities();
            var bl = new DogBreedBL();

            var breed = bl.GetBreedById(id, db);
            if (breed == null)
                return HttpNotFound();

            var subBreeds = bl.GetSubBreeds(id, db);

            if (subBreeds.Any())
            {
                return Json(new
                {
                    hasSubBreeds = true,
                    message = $"'{breed.DogName}' has {subBreeds.Count} sub-breed(s). Deleting this will also delete them. Proceed?",
                    breedId = breed.PK_DogBreedId
                });
            }

            bool success = bl.DeleteBreed(breed, db);
            return Json(new { success  = true});
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public ActionResult CascadeDelete(int id)
        {
            var db = new DogHubEntities();
            var bl = new DogBreedBL();

            var parent = bl.GetBreedById(id, db);
            if (parent == null)
                return HttpNotFound();

            var subs = bl.GetSubBreeds(id, db);
            bool success = bl.DeleteBreedWithSubBreeds(parent, subs, db);

            return Json(new { success = true});
        }


        protected override void Dispose(bool disposing)
        {
            if (disposing)
                db.Dispose();

            base.Dispose(disposing);
        }
    }
}
