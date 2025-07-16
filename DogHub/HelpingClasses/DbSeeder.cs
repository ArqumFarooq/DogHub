using DogHub.BL;
using DogHub.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;

namespace DogHub.HelpingClasses
{
    public class DbSeeder
    {
        public static void SeedDatabase(DogHubEntities db)
        {
            SeedAdminUser(db);
            SeedDogBreeds(db);
        }

        private static void SeedAdminUser(DogHubEntities db)
        {
            var userBL = new UserBL();
            bool isSeeded = userBL.Seed_AdminUser(db);

            if (isSeeded)
            {
                var auditBL = new AuditLogBL();
                auditBL.AddAuditLog(
                    actionType: "Seed",
                    affectedTable: "Users",
                    auditDetails: "Default admin user seeded.",
                    createdById: null,
                    de: db
                );
            }
        }

        private static void SeedDogBreeds(DogHubEntities db)
        {
            if (db.DogBreeds.Any()) return; // Avoid reseeding

            string jsonPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"App_Data\dog.json");
            if (!File.Exists(jsonPath)) return;

            var dogBL = new DogBreedBL();
            bool isSeeded = dogBL.Seed_LoadDogBreedsFromJson(jsonPath, db);

            if (isSeeded)
            {
                var auditBL = new AuditLogBL();
                auditBL.AddAuditLog(
                    actionType: "Seed",
                    affectedTable: "DogBreeds",
                    auditDetails: "Dog breeds seeded from JSON file.",
                    createdById: null, 
                    de: db
                );
            }
        }
    }
}