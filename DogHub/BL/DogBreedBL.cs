using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DogHub.DAL;
using DogHub.Models;
using Newtonsoft.Json;

namespace DogHub.BL
{
    public class DogBreedBL
    {
        private readonly DogBreedDAL dal = new DogBreedDAL();

        public List<DogBreed> GetAllDogBreeds(DogHubEntities db)
        {
            return dal.GetAllDogBreeds(db);
        }

        public DogBreed GetDogBreedById(int id, DogHubEntities db)
        {
            return dal.GetDogBreedById(id, db);
        }

        public bool AddDogBreed(DogBreed breed, DogHubEntities db)
        {
            return dal.AddDogBreed(breed, db);
        }
        public DogBreed AddAndGetDogBreed(DogBreed dogBreed, DogHubEntities db)
        {
            return dal.AddAndReturnDogBreed(dogBreed, db);
        }

        public bool UpdateDogBreed(DogBreed breed, DogHubEntities db)
        {
            return dal.UpdateDogBreed(breed, db);
        }
        public bool UpdateDogBreedUsingId(int id, DogBreed updatedBreed, DogHubEntities db)
        {
            return dal.UpdateDogBreedById(id, updatedBreed, db);
        }

        public bool DeleteDogBreed(int id, DogHubEntities db)
        {
            return dal.DeleteDogBreed(id, db);
        }

        #region casecade deletion
        public DogBreed GetBreedById(int id, DogHubEntities db)
        {
            return dal.GetBreedById(id, db);
        }

        public List<DogBreed> GetSubBreeds(int parentId, DogHubEntities db)
        {
            return dal.GetSubBreeds(parentId, db);
        }

        public bool DeleteBreed(DogBreed breed, DogHubEntities db)
        {
            return dal.SoftDeleteBreed(breed, db);
        }

        public bool DeleteBreedWithSubBreeds(DogBreed parent, List<DogBreed> subs, DogHubEntities db)
        {
            return dal.SoftDeleteBreedWithSubBreeds(parent, subs, db);
        }

        #endregion

        #region Seed_DogBreed

        public bool Seed_LoadDogBreedsFromJson(string jsonPath, DogHubEntities db)
        {
            if (dal.Seed_AnyDogBreeds(db)) return false;
            if (!File.Exists(jsonPath)) return false;

            var json = File.ReadAllText(jsonPath);
            var dict = JsonConvert.DeserializeObject<Dictionary<string, List<string>>>(json);
            if (dict == null) // JSON Schema validation
            {
                return false;
            }
            bool isSeeded = false;

            foreach (var kvp in dict)
            {
                string mainBreed = kvp.Key;
                List<string> subBreeds = kvp.Value;

                var parent = new DogBreed
                {
                    DogName = Capitalize(mainBreed),
                    ParentBreedId = null,
                    Origin = "Unknown",
                    LifeSpan = "Unknown",
                    Description = $"The {mainBreed} breed",
                    ImageUrl = null,
                    SysCreatedDate = DateTime.Now,
                    SysCreatedID = 1,
                    IsDeleted = false
                };

                parent = dal.Seed_AddDogBreed(parent, db);

                if (parent == null) continue; // failed to insert parent

                var subBreedList = subBreeds.Select(sub => new DogBreed
                {
                    DogName = $"{Capitalize(sub)} {Capitalize(mainBreed)}",
                    ParentBreedId = parent.PK_DogBreedId,
                    Origin = "Unknown",
                    LifeSpan = "Unknown",
                    Description = $"A sub-breed of {mainBreed}",
                    ImageUrl = null,
                    SysCreatedDate = DateTime.Now,
                    SysCreatedID = 1,
                    IsDeleted = false
                }).ToList();

                if (subBreedList.Any())
                {
                    dal.Seed_AddDogBreeds(subBreedList, db);
                }

                isSeeded = true;
            }

            return isSeeded;
        }

        private string Capitalize(string str)
        {
            if (string.IsNullOrWhiteSpace(str)) return str;
            return char.ToUpper(str[0]) + str.Substring(1).ToLower();
        }
        #endregion
    }
}
