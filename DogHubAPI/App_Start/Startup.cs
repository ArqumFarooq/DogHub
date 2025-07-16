using System;
using System.Web.Http;
using Owin;
using Microsoft.Owin;
using Microsoft.Owin.Security.OAuth;
using DogHubAPI.AuthorizeProvider;

[assembly: OwinStartup(typeof(DogHubAPI.App_Start.Startup))]

namespace DogHubAPI.App_Start
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            // Enable CORS (for Swagger/Postman)
            app.UseCors(Microsoft.Owin.Cors.CorsOptions.AllowAll);

            // OAuth Token Configuration
            OAuthAuthorizationServerOptions oAuthOptions = new OAuthAuthorizationServerOptions
            {
                AllowInsecureHttp = true, // Use HTTPS in production and turn it 'False'
                TokenEndpointPath = new PathString("/token"),
                AccessTokenExpireTimeSpan = TimeSpan.FromHours(1),
                Provider = new SimpleAuthorizationServerProvider()
            };

            // Enable OAuth Bearer Token Authentication
            app.UseOAuthAuthorizationServer(oAuthOptions);
            app.UseOAuthBearerAuthentication(new OAuthBearerAuthenticationOptions());

            // Setup Web API
            HttpConfiguration config = new HttpConfiguration();

            WebApiConfig.Register(config);
            SwaggerConfig.Register(config);

            app.UseWebApi(config);

            // Redirect base URL to Swagger
            app.Use(async (context, next) =>
            {
                if (context.Request.Path.Value == "/")
                {
                    context.Response.Redirect("/swagger");
                    return;
                }

                await next();
            });
        }
    }
}
