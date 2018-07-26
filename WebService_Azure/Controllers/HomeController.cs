using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using System.Web.Mvc;
using System.Globalization;
using System.Threading.Tasks;
using System.Threading;
using Newtonsoft.Json.Linq;
using System.Net.Http;
using System.Net;
using Newtonsoft.Json;
using System.Net.Http.Headers;
using System.IO;

namespace WebService_Azure.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }
        
        public async Task<JsonResult> GetOppIDResult(string id)
        {
            string url = @"https://dxctnpapiprod.azurewebsites.net/FD/api/opp/" + id + "?appName=FrontDoorSICReporting";
            string status = "";
            var client = new HttpClient()
            {
                DefaultRequestHeaders = { Authorization = new AuthenticationHeaderValue("Bearer", await GetBearerAuthValue()) }
            };
            try
            {
                HttpResponseMessage rsd = await client.GetAsync(url);
                HttpStatusCode statusCode = rsd.StatusCode;
                status = statusCode.ToString();
                string ErrorCode = GenerateErrorCode(status);
                string OppName = "";
                string jsonStr = await rsd.Content.ReadAsStringAsync();
                if (jsonStr != "")
                {
                    jsonStr = jsonStr.Replace("\\\"", "").Replace("\"", "").Replace("{", "").Replace("}", "");
                    OppName = jsonStr.Split(',').ToList().Find(ele => ele.Contains("OpportunityName")).Split(':')[1];
                }
                var res = new
                {
                    OppName = OppName,
                    ErrorCode = ErrorCode,
                    ErrorMessage = "no exception"
                };
                return Json(res, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                var exRes = new
                {
                    OppName = "",
                    ErrorCode = "Exception",
                    ErrorMessage = ex.Message
                };
                return Json(exRes, JsonRequestBehavior.AllowGet);
            }
            
        }
        private string GenerateErrorCode(string status)
        {
            switch(status){
                case "OK":
                    return "ERROR_CODE_0";
                case "NotFound":
                    return "ERROR_CODE_12";
                case "RequestTimeout":
                    return "ERROR_CODE_998";
                case "BadRequest":
                    return "BadRequest";
                case "Unauthorized":
                    return "Unauthorized";
                case "Forbidden":
                    return "Forbidden";
                case "MethodNotAllowed":
                    return "MethodNotAllowed";
                case "Gone":
                    return "Gone";
                case "Conflict":
                    return "Conflict";
                case "InternalServerError":
                    return "InternalServerError";
                default:
                    return "uncatch case";

            }
        }
        public async Task<string> GetBearerAuthValue()
        {
            //string authorization = string.Format("{0}:{1}", username, password);
            //return Convert.ToBase64String(new ASCIIEncoding().GetBytes(authorization));
            string tenant = "cscportal.onmicrosoft.com";
            string clientId = "ecf17ca2-5c9e-4f1c-86cb-a320bf70ea02";
            string appKey = "xLMHIN3v5+OypJ5Vf0aDx9W6JClQZPNYdi9DByFG4eo=";
            string aadInstance = "https://login.microsoftonline.com/{0}";
            string resourceId = "ecf17ca2-5c9e-4f1c-86cb-a320bf70ea02";
            string authority = String.Format(CultureInfo.InvariantCulture, aadInstance, tenant);
            var authContext = new AuthenticationContext(authority);
            var clientCredential = new ClientCredential(clientId, appKey);
            AuthenticationResult result = null;
            int retryCount = 0;
            bool retry = false;
            do
            {
                retry = false;
                try
                {
                    // ADAL includes an in memory cache, so this call will only send a message to the server if the cached token is expired.
                    //result = authContext.AcquireToken(resourceId, clientCredential);
                    result = await authContext.AcquireTokenAsync(resourceId, clientCredential);
                }
                catch (AdalException ex)
                {
                    if (ex.ErrorCode == "temporarily_unavailable")
                    {
                        retry = true;
                        retryCount++;
                        Thread.Sleep(3000);
                    }
                }
                catch (Exception e)
                {

                }

            } while ((retry == true) && (retryCount < 3));

            if (result == null)
            {
                return null;
            }
            return result.AccessToken;
        }
    }
}