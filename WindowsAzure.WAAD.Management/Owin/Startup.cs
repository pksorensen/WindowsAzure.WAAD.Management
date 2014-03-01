using Microsoft.IdentityModel.Clients.ActiveDirectory;
using Microsoft.Owin;
using Microsoft.Owin.Security.Cookies;
using Microsoft.Owin.Security.WsFederation;
using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Auth;
using Microsoft.WindowsAzure.Storage.Blob;
using Newtonsoft.Json;
using Owin;
using System;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using System.Web;

[assembly: OwinStartup(typeof(WindowsAzure.WAAD.Management.Owin.Startup))]

namespace WindowsAzure.WAAD.Management.Owin
{
    public class Startup
    { 
         public void Configuration(IAppBuilder app)
         {
             // For more information on how to configure your application, visit http://go.microsoft.com/fwlink/?LinkID=316888


             app.UseCookieAuthentication(new CookieAuthenticationOptions
             {
                 AuthenticationType = WsFederationAuthenticationDefaults.AuthenticationType
             });
             app.UseWsFederationAuthentication(new WsFederationAuthenticationOptions
             {
                 TokenValidationParameters = new System.IdentityModel.Tokens.TokenValidationParameters
                 {
                     //UPDATE Bug has been confirmed that this is not working as intented.
                     // I will update this demo as more information is avalibe.
                     //ValidateIssuer  = false -> this causes IssuereValidator to not be run. 
                     IssuerValidator = (issuer, token) =>
                     {
                         return false; // it still sign in the user even though its false.

                         //     return DatabaseIssuerNameRegistry.ContainsTenant(issuer);

                     }
                 },
                 Wtrealm = "https://cvservice.s-innovations.net",
                 MetadataAddress = "https://login.windows.net/802626c6-0f5c-4293-a8f5-198ecd481fe3/FederationMetadata/2007-06/FederationMetadata.xml"
             });

             app.Map("/login", OwinControllers.Login);
             app.Map("/logout", OwinControllers.Logout);
             app.Map("/signup", OwinControllers.Signup);
             app.Map("/signup-callback", OwinControllers.SignupCallback);

             app.Map("/api/getAuthorizationUri", OwinControllers.GetAuthorizationUri);
             app.Map("/api/getUserInfo", OwinControllers.GetUserInfo);
             app.Map("/api/getAccessToken", OwinControllers.GetAccessToken);
             app.Map("/api/getStorageAccountInfo", OwinControllers.GetStorageAccountInfo);
             app.Map("/api/getStorageAccounts", OwinControllers.GetStorageAccounts);
         }
    }
}
