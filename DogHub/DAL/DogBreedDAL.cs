using System;
using System.Collections.Generic;
using System.Data.Entity.Validation;
using System.Linq;
using DogHub.Models;

namespace DogHub.DAL
{
    public class DogBreedDAL
    {
        #region DogBreed

        public List<DogBreed> GetAllDogBreeds(DogHubEntities db)
        {
            return db.DogBreeds.Where(x => x.IsDeleted == false || x.IsDeleted == null).ToList();
        }

        public DogBreed GetDogBreedById(int id, DogHubEntities db)
        {
            return db.DogBreeds.FirstOrDefault(x => x.PK_DogBreedId == id && (x.IsDeleted == false || x.IsDeleted == null));
        }

        public bool AddDogBreed(DogBreed breed, DogHubEntities db)
        {
            try
            {
                db.DogBreeds.Add(breed);
                db.SaveChanges();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public DogBreed AddAndReturnDogBreed(DogBreed dogBreed, DogHubEntities db)
        {
            db.DogBreeds.Add(dogBreed);
            try
            {
                db.SaveChanges();
                return dogBreed;
            }
            catch (DbEntityValidationException ex)
            {
                foreach (var eve in ex.EntityValidationErrors)
                {
                    Console.WriteLine("Entity of type \"{0}\" in state \"{1}\" has the following validation errors:",
                        eve.Entry.Entity.GetType().Name, eve.Entry.State);

                    foreach (var ve in eve.ValidationErrors)
                    {
                        Console.WriteLine("- Property: \"{0}\", Error: \"{1}\"",
                            ve.PropertyName, ve.ErrorMessage);
                    }
                }

                throw; // rethrow to preserve stack trace
            }
        }

        public bool UpdateDogBreed(DogBreed breed, DogHubEntities db)
        {
            try
            {
                db.Entry(breed).State = System.Data.Entity.EntityState.Modified;
                db.SaveChanges();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public bool UpdateDogBreedById(int id, DogBreed updatedBreed, DogHubEntities db)
        {
            var existing = db.DogBreeds.FirstOrDefault(x => x.PK_DogBreedId == id && x.DogName == updatedBreed.DogName);
            if (existing == null)
                return false;

            existing.DogName = updatedBreed.DogName;
            existing.Origin = updatedBreed.Origin;
            existing.LifeSpan = updatedBreed.LifeSpan;
            existing.Description = updatedBreed.Description;
            existing.ImageUrl = updatedBreed.ImageUrl;

            db.SaveChanges();
            return true;
        }

        public bool DeleteDogBreed(int id, DogHubEntities db)
        {
            try
            {
                var breed = db.DogBreeds.Find(id);
                if (breed == null) return false;

                breed.IsDeleted = true;
                breed.SysModifiedDate = DateTime.Now;
                db.Entry(breed).State = System.Data.Entity.EntityState.Modified;
                db.SaveChanges();
                return true;
            }
            catch
            {
                return false;
            }
        }
        #endregion

        #region casecade deletion
        public DogBreed GetBreedById(int id, DogHubEntities db)
        {
            return db.DogBreeds.FirstOrDefault(b => b.PK_DogBreedId == id && (b.IsDeleted == false || b.IsDeleted == null));
        }

        public List<DogBreed> GetSubBreeds(int parentId, DogHubEntities db)
        {
            return db.DogBreeds.Where(b => b.ParentBreedId == parentId && (b.IsDeleted == false || b.IsDeleted == null)).ToList();
        }

        public bool SoftDeleteBreed(DogBreed breed, DogHubEntities db)
        {
            try
            {
                breed.IsDeleted = true;
                breed.SysModifiedDate = DateTime.Now;
                db.Entry(breed).State = System.Data.Entity.EntityState.Modified;
                db.SaveChanges();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public bool SoftDeleteBreedWithSubBreeds(DogBreed parent, List<DogBreed> subs, DogHubEntities db)
        {
            try
            {
                foreach (var sub in subs)
                {
                    sub.IsDeleted = true;
                    sub.SysModifiedDate = DateTime.Now;
                    db.Entry(sub).State = System.Data.Entity.EntityState.Modified;
                }

                parent.IsDeleted = true;
                parent.SysModifiedDate = DateTime.Now;
                db.Entry(parent).State = System.Data.Entity.EntityState.Modified;

                db.SaveChanges();
                return true;
            }
            catch
            {
                return false;
            }
        }

        #endregion

        #region Seed_DogBreed
        public bool Seed_AnyDogBreeds(DogHubEntities db)
        {
            return db.DogBreeds.Any();
        }

        public DogBreed Seed_AddDogBreed(DogBreed breed, DogHubEntities db)
        {
            try
            {
                db.DogBreeds.Add(breed);
                db.SaveChanges();
                return breed;
            }
            catch
            {
                return null;
            }
        }

        public bool Seed_AddDogBreeds(List<DogBreed> breeds, DogHubEntities db)
        {
            try
            {
                db.DogBreeds.AddRange(breeds);
                db.SaveChanges();
                return true;
            }
            catch
            {
                return false;
            }
        }
        #endregion

    }
}
