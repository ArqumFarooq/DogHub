using System;
using System.Web.Helpers;
using Owin;
using Microsoft.Owin;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.Cookies;
using Microsoft.Owin.Security.Google;
using System.Configuration;
using Microsoft.AspNet.Identity;

[assembly: OwinStartup(typeof(DogHub.App_Start.Startup))]

namespace DogHub.App_Start
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            // Set default authentication type (optional)
            app.SetDefaultSignInAsAuthenticationType(CookieAuthenticationDefaults.AuthenticationType);

            // Enable cookie authentication
            app.UseCookieAuthentication(new CookieAuthenticationOptions
            {
                AuthenticationType = "ApplicationCookie",  //Identifies this cookie for login/logout
                LoginPath = new PathString("/Auth/Login"), //unauthenticated users are redirected
                ExpireTimeSpan = TimeSpan.FromMinutes(30), //Cookie/session timeout
                SlidingExpiration = true                   //Refresh cookie timeout on activity
            });

            // Enables temporary external cookie storage
            app.UseExternalSignInCookie(DefaultAuthenticationTypes.ExternalCookie);

            // Google authentication
            app.UseGoogleAuthentication(new GoogleOAuth2AuthenticationOptions()
            {
                ClientId = ConfigurationManager.AppSettings["GoogleClientId"],
                ClientSecret = ConfigurationManager.AppSettings["GoogleClientSecret"],
                CallbackPath = new PathString("/signin-google"),
                Scope = { "email", "profile" }
            });

            // Needed for AntiForgery with claims-based identity
            AntiForgeryConfig.UniqueClaimTypeIdentifier = System.Security.Claims.ClaimTypes.Sid; //Links AntiForgery tokens with Claims Identity
        }
    }
}
