using System.Web.Http;
using WebActivatorEx;
using DogHubAPI;
using Swashbuckle.Application;

namespace DogHubAPI
{
    public class SwaggerConfig
    {
        public static void Register(HttpConfiguration config)
        {
            config
                .EnableSwagger(c =>
                {
                    c.SingleApiVersion("v1", "DogHubAPI");

                    // Enable JWT Bearer token input
                    c.ApiKey("Bearer")
                        .Description("JWT Authorization header using the Bearer scheme. Example: \"Bearer {token}\"")
                        .Name("Authorization")
                        .In("header");
                })
                .EnableSwaggerUi(c =>
                {
                    c.EnableApiKeySupport("Authorization", "header");
                });
        }
    }
}
