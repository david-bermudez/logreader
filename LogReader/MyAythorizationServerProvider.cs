using Microsoft.Owin.Security.OAuth;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Threading.Tasks;
using System.Security.Claims;
using Microsoft.Owin;
using System.Configuration;
using System.Web.Configuration;
using System.Net;
using System.Net.Http;
using System.IO;
using System.DirectoryServices;

namespace LogReader
{
    /* Historia
    * 2022-03-22 v2.09 Se agrega metodo para obtener el password sin econding.
    */
    public class MyAythorizationServerProvider : OAuthAuthorizationServerProvider
    {

        public static string PASSWORD = "password=";
        public override async Task ValidateClientAuthentication(OAuthValidateClientAuthenticationContext context)
        {
            context.Validated();
            //return base.ValidateClientAuthentication(context);
        }

        public override async Task GrantResourceOwnerCredentials(OAuthGrantResourceOwnerCredentialsContext context)
        {
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            string requestBody = GetRequestBody();
            string rawPassword = GetBodyPassword(requestBody);
            try
            {
                
                //context.OwinContext.Response.Headers.Add("Access-Control-Allow-Origin", new[] { "*" });
                var identity = new ClaimsIdentity(context.Options.AuthenticationType);

                IFormCollection parameters = await context.Request.ReadFormAsync();
                var district = parameters.Get("district");
                var position = parameters.Get("position");

                String LDAPDomainName = GetLDAPDomainName();
                bool userAuthenticated = false;

                if (LDAPDomainName != null)
                {
                    userAuthenticated = AuthenticateUser(LDAPDomainName, context.UserName, context.Password);
                } else
                {
                    userAuthenticated = true;
                }

                if (!userAuthenticated)
                {
                    context.SetError("invalid_grant", "Incorrect Credentials");
                }
                else
                {
                    identity.AddClaim(new Claim(ClaimTypes.Role, "admin"));
                    identity.AddClaim(new Claim("username", context.UserName));
                    identity.AddClaim(new Claim("pwd", EllipseWebServicesClient.ClientConversation.password));
                    identity.AddClaim(new Claim("district", district));
                    identity.AddClaim(new Claim("position", position));
                    context.Validated(identity);
                }
            }
            catch (Exception ex)
            {
                context.SetError("invalid_grant", ex.Message);
            }

        }

        private string GetBodyPassword(string requestBody)
        {
            if (requestBody != null)
            {
                foreach (string param in requestBody.Split('&'))
                {
                    if (param.Contains(PASSWORD))
                    {
                        try
                        {
                            return param.Split('=')[1];
                        } catch (Exception)
                        {
                            return "";
                        }
                    }
                }
            }

            return "";
        }

        public static string GetRequestBody()
        {
            var bodyStream = new StreamReader(HttpContext.Current.Request.InputStream);
            bodyStream.BaseStream.Seek(0, SeekOrigin.Begin);
            var bodyText = bodyStream.ReadToEnd();
            return bodyText;
        }

        private string GetLDAPDomainName() {
            try
            {
                return WebConfigurationManager.AppSettings["LDAPDomainName"];
            } catch (Exception)
            {
                return null;
            }
        }

        private bool AuthenticateUser(string domainName, string userName, string password)
        {
            bool ret = false;

            try
            {
                DirectoryEntry de = new DirectoryEntry("LDAP://" + domainName, userName, password);
                DirectorySearcher dsearch = new DirectorySearcher(de);
                SearchResult results = null;

                results = dsearch.FindOne();

                ret = true;
            }
            catch
            {
                ret = false;
            }

            return ret;
        }
    }
}