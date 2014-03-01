using Microsoft.IdentityModel.Clients.ActiveDirectory;
using Microsoft.Owin;
using Microsoft.Owin.Security.Cookies;
using Microsoft.Owin.Security.WsFederation;
using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Auth;
using Microsoft.WindowsAzure.Storage.Blob;
using Microsoft.WindowsAzure.Subscriptions;
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
        private const string ConsentUrlFormat = "https://account.activedirectory.windowsazure.com/Consent.aspx?ClientId={0}";
         private static readonly string AppPrincipalId = CloudConfigurationManager.GetSetting("ida:ClientID");
         private static readonly string AppKey = CloudConfigurationManager.GetSetting("ida:Password");
         private const string TenantIdClaimType = "http://schemas.microsoft.com/identity/claims/tenantid";
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
            app.Map("/login", map =>
            {
                map.Run(async ctx =>
                {


                    if (ctx.Authentication.User == null ||
                        !ctx.Authentication.User.Identity.IsAuthenticated)
                    {

                        ctx.Response.StatusCode = 401;
                    }
                    else
                    {
                        ctx.Response.Redirect("/");
                    }
                });
            });

            app.Map("/logout", map =>
            {
                map.Run(async ctx =>
                {
                    ctx.Authentication.SignOut();
                    ctx.Response.Redirect("/");
                });

            });
            app.Map("/api/getAuthorizationUri", map =>
            {
                map.Run(async ctx =>
                {
                    if (!ctx.Authentication.User.Identity.IsAuthenticated)
                    {
                        ctx.Response.StatusCode = (int)HttpStatusCode.Forbidden;
                        return;
                    }
                    string authorizationUrl = string.Format(
                      "https://login.windows.net/{0}/oauth2/authorize?api-version=1.0&response_type=code&client_id={1}&resource={2}&redirect_uri={3}",
                    ClaimsPrincipal.Current.FindFirst(TenantIdClaimType).Value,
                     AppPrincipalId,
                     "https://management.core.windows.net/",
                    new Uri(ctx.Request.Uri, "/"));
                    await ctx.Response.WriteAsync(authorizationUrl);
                });
            });
            app.Map("/api/getUserInfo", map =>
            {
                map.Run(async ctx =>
                {
                    if (!ctx.Authentication.User.Identity.IsAuthenticated)
                    {
                        ctx.Response.StatusCode = (int)HttpStatusCode.Forbidden;
                        return;
                    }

                    await ctx.Response.WriteAsync(ctx.Authentication.User.Identity.Name);
                });
            });
            app.Map("/signup", map =>
            {
                map.Run(async ctx =>
                {
                    string signupToken = Guid.NewGuid().ToString();
                    UriBuilder replyUrl = new UriBuilder(ctx.Request.Uri);
                    replyUrl.Path = "/signup-callback";
                    replyUrl.Query = "signupToken=" + signupToken;


                    ctx.Response.Redirect(CreateConsentUrl(
                        clientId: AppPrincipalId,
                        requestedPermissions: "DirectoryReaders",//, CompanyAdministrator",
                        consentReturnUrl: replyUrl.Uri.AbsoluteUri));
                });
            });
            app.Map("/signup-callback", map =>
            {
                map.Run(async ctx =>
                {
                    string tenantId = ctx.Request.Query["TenantId"];
                    string signupToken = ctx.Request.Query["signupToken"];


                    string consent = ctx.Request.Query["Consent"];

                    if (!String.IsNullOrEmpty(tenantId) &&
                        String.Equals(consent, "Granted", StringComparison.OrdinalIgnoreCase))
                    {

                        ctx.Response.Redirect("/");
                    }
                    await ctx.Response.WriteAsync(@"Sorry, you have to press accept :)");
                });
            });
            app.Map("/api/getAccessToken", map =>
            {
                map.Run(async ctx =>
                {
                    if (!ctx.Authentication.User.Identity.IsAuthenticated)
                    {
                        ctx.Response.Redirect("/login");
                        return;
                    }

                    string code = ctx.Request.Query["code"];

                    if (code == null)
                    {
                        ctx.Response.Redirect("/");
                        return;
                    }

                    AuthenticationContext ac =
                        new AuthenticationContext(string.Format("https://login.windows.net/{0}",
                            ctx.Authentication.User.FindFirst(TenantIdClaimType).Value));


                    UriBuilder replyUrl = new UriBuilder(ctx.Request.Uri);
                    replyUrl.Path = "/";
                    replyUrl.Query = "";
                    ClientCredential clcred =
                        new ClientCredential(AppPrincipalId, AppKey);
                    AuthenticationResult ar = null;
                    try
                    {
                        ar = ac.AcquireTokenByAuthorizationCode(code,
                           replyUrl.Uri, clcred);
                    }
                    catch (Exception)
                    {
                        ctx.Response.Redirect("/login");
                        return;
                    }

                    await ctx.Response.WriteAsync(ar.AccessToken);
                });
            });


            app.Map("/api/getStorageAccountInfo", map =>
            {

                map.Run(async ctx =>
                {
                    if (!ctx.Authentication.User.Identity.IsAuthenticated)
                    {
                        ctx.Response.Redirect("/login");
                        return;
                    }

                    var Authorization = ctx.Request.Headers.Get("Authorization");
                    var parts = Authorization.Split(' ');
                    var token = parts.Last();
                    string account = ctx.Request.Query["account"];

                    ctx.Response.ContentType = "text/json";

                    using (var azure = new SubscriptionClient(new TokenCloudCredentials(token)))
                    {
                        var subscriptions = await azure.Subscriptions.ListAsync();

                        var tasks = await Task.WhenAll(subscriptions.Select(id => new TokenCloudCredentials(id.SubscriptionId, token))
                            .Select(async cred =>
                            {
                                using (var storage = CloudContext.Clients.CreateStorageManagementClient(cred))
                                {
                                    var storages = await storage.StorageAccounts.ListAsync(new CancellationToken());
                                    if (storages.Any(s=>s.ServiceName.Equals(account)))
                                    {

                                        return await storage.StorageAccounts.GetKeysAsync(account, new CancellationToken());

                                    }
                                    return null;
                                }
                            }));
                        var key = tasks.First(s => s != null).PrimaryKey;

                        var blobs = new CloudStorageAccount(new StorageCredentials(account, key), true);
                        var blobClient = blobs.CreateCloudBlobClient();
                        try
                        {
                            var allblobs = blobClient.ListContainers().SelectMany(c => c.ListBlobs(null, true, BlobListingDetails.Metadata).OfType<CloudBlockBlob>());

                            var bytesize = allblobs.Aggregate(0L, (value, b) => value + b.Properties.Length);
                            await ctx.Response.WriteAsync(JsonConvert.SerializeObject(new { Bytes = bytesize }));

                        }catch(Exception ex)
                        {
                            Trace.TraceError(ex.ToString());
                            var a = ex;
                        }
     

                    }

                });
            });
            
            app.Map("/api/getStorageAccounts", map =>
            {

                map.Run(async ctx =>
                {
                    if (!ctx.Authentication.User.Identity.IsAuthenticated)
                    {
                        ctx.Response.Redirect("/login");
                        return;
                    }

                    var Authorization = ctx.Request.Headers.Get("Authorization");
                    var parts = Authorization.Split(' ');
                    var token = parts.Last();
                    

                    ctx.Response.ContentType = "text/json";



                    using (var azure = new SubscriptionClient(new TokenCloudCredentials(token)))
                    {
                        var subscriptions = await azure.Subscriptions.ListAsync();

                        var tasks = await Task.WhenAll(subscriptions.Select(id => new TokenCloudCredentials(id.SubscriptionId, token))
                            .Select(async cred =>
                         {
                             using (var storage = CloudContext.Clients.CreateStorageManagementClient(cred))
                             {
                                // var loc = storage.StorageAccounts.GetKeys("sinnovations"); // WORKS
                                 var storages = await storage.StorageAccounts.ListAsync(new CancellationToken());
                                 // storages.Count is 0

                                 return storages.Select(s => s.ServiceName);
                             }
                         }));
                        await ctx.Response.WriteAsync(JsonConvert.SerializeObject(tasks.SelectMany(s => s).ToArray()));

                    }
                  

                });

            });
        }
               
        private string CreateConsentUrl(string clientId, string requestedPermissions,
                                  string consentReturnUrl)
        {
            string consentUrl = String.Format(
                CultureInfo.InvariantCulture,
                ConsentUrlFormat,
                HttpUtility.UrlEncode(clientId));

            if (!String.IsNullOrEmpty(requestedPermissions))
            {
                consentUrl += "&RequestedPermissions=" + HttpUtility.UrlEncode(requestedPermissions);
            }
            if (!String.IsNullOrEmpty(consentReturnUrl))
            {
                consentUrl += "&ConsentReturnURL=" + HttpUtility.UrlEncode(consentReturnUrl);
            }

            return consentUrl;
        }
    }
}
