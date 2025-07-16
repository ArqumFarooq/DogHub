using DogHub.BL;
using DogHub.HelpingClasses;
using DogHub.Models;
using System;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;

namespace DogHub
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
            #if DEBUG
                 SeedDatabaseOnStart();
            #endif
        }

        private void SeedDatabaseOnStart()
        {
            using (var db = new DogHubEntities())
            {
                try
                {
                    DbSeeder.SeedDatabase(db);
                }
                catch (Exception ex)
                {
                    try
                    {
                        new AuditLogBL().AddAuditLog(
                            actionType: "Error",
                            affectedTable: "Seeder",
                            auditDetails: $"Database seeding failed: {ex.Message}",
                            createdById: null,
                            de: db
                        );
                    }
                    catch
                    {
                    }
                }
            }
        }
    }
}
