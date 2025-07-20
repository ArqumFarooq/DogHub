using System.Collections.Generic;
using System.Web.Http;
using DogHub.BL;
using DogHub.Models;

namespace DogHubAPI.Controllers
{
    [Authorize]
    [RoutePrefix("api/dogbreeds")]
    public class DogBreedController : ApiController
    {
        private readonly DogBreedBL _bl = new DogBreedBL();
        private readonly DogHubEntities db = new DogHubEntities();

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

        // GET: api/dogbreeds
        [HttpGet]
        [Route("")]
        public IHttpActionResult GetAll()
        {
            var breeds = _bl.GetAllDogBreeds(db);
            return Ok(breeds);
        }

        // GET: api/dogbreeds/{id}
        [HttpGet]
        [Route("{id:int}")]
        public IHttpActionResult GetById(int id)
        {
            var breed = _bl.GetDogBreedById(id, db);
            if (breed == null)
                return NotFound();

            return Ok(breed);
        }

        // POST: api/dogbreeds
        [HttpPost]
        [Route("")]
        public IHttpActionResult Create(DogBreed dogBreed)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var addedBreed = _bl.AddAndGetDogBreed(dogBreed, db);
            LogAudit("AddDogBreed", "DogBreeds", $"DogBreed Record Added Successfully via WebAPI: {addedBreed.DogName}", 0);
            return CreatedAtRoute("DefaultApi", new { id = addedBreed.PK_DogBreedId }, addedBreed);
        }

        // PUT: api/dogbreeds/{id}
        [HttpPut]
        [Route("{id:int}")]
        public IHttpActionResult Update(int id, DogBreed updatedBreed)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = _bl.UpdateDogBreedUsingId(id, updatedBreed, db);
            if (!result)
            {
                return NotFound();
            }
            LogAudit("UpdateDogBreed", "DogBreeds", $"DogBreed Record Updated Successfully via WebAPI", 0);
            return Ok("Updated successfully.");
        }

        // DELETE: api/dogbreeds/{id}
        [HttpDelete]
        [Route("{id:int}")]
        public IHttpActionResult Delete(int id)
        {
            var result = _bl.DeleteDogBreed(id, db);
            if (!result)
            {
                return NotFound();
            }
            LogAudit("DeletDogBreed", "DogBreeds", $"DogBreed Record Deleted Successfully via WebAPI", 0);
            return Ok("Deleted successfully.");
        }
    }
}
