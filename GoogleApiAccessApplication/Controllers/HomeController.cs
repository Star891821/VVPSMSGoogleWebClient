using GoogleApiAccessApplication.Common;
using GoogleApiAccessApplication.Models;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Diagnostics;
using System.Xml;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace GoogleApiAccessApplication.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Authenticate()
        {
            string email = Request.Form["email"].FirstOrDefault();
            return RedirectPermanent(GoogleApiHelper.GetOauthUri(email));
        }
        public async Task<IActionResult> OauthCallback(string code, string error, string state)
        {
            Task< LoginAuthResponse> loginAuthResponseTask = null;
            try
            {
                if (!string.IsNullOrEmpty(code))
                {
                    var token = await GoogleApiHelper.GetTokenByCode(code);
                    if (token != null)
                    {
                        //ViewBag.AccessToken = token.access_token;
                        //ViewBag.RefreshToken = token.refresh_token;
                        //ViewBag.IdToken = token.id_token;

                        using (HttpClient client = new HttpClient())
                        {
                            //parametrai (PARAMS of your call)
                            var parameters = new Dictionary<string, string> { { "username", "YOURUSERNAME" }, { "password", "YOURPASSWORD" } };
                            //Uzkoduojama URL'ui 
                            var encodedContent = new FormUrlEncodedContent(parameters);
                            try
                            {
                                UserView obj = new UserView
                                {
                                    tokenId = token.id_token
                                };
                                string json = JsonConvert.SerializeObject(obj);   //using Newtonsoft.Json

                                StringContent httpContent = new StringContent(json, System.Text.Encoding.UTF8, "application/json");
                                StringContent queryString = new StringContent(json);
                                //Post http callas.
                                HttpResponseMessage response = client.PostAsync("https://localhost:7188/api/ExternalLogin/Google/oAuthGoogleToken", httpContent).Result;
                                //nesekmes atveju error..
                                response.EnsureSuccessStatusCode();
                                //responsas to string
                                string responseBody = response.Content.ReadAsStringAsync().Result;

                                ViewBag.responseBody = responseBody;
                                var resp = response.Content.ReadFromJsonAsync<LoginAuthResponse>();                              
                                ViewBag.LoginAuthResponse = resp;
                                loginAuthResponseTask = resp;
                            }
                            catch (HttpRequestException e)
                            {
                                Console.WriteLine("\nException Caught!");
                                Console.WriteLine("Message :{0} ", e.Message);
                            }

                        }
                    }
                   
                }
                if (!string.IsNullOrEmpty(error))
                {
                    ViewBag.Error = "Error :" + error;
                }
                if (!string.IsNullOrEmpty(state))
                {
                    ViewBag.MailAddress = state;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return View(loginAuthResponseTask);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }

    public class UserView
    {
        public string tokenId { get; set; }
    }

    public class LoginAuthResponse
    {
        public string? jwtToken { get; set; }

        public string? expiryDateTime { get; set; }

        public int userId { get; set; }
        public string? userName { get; set; }

        public string? givenName { get; set; }

        public string? surName { get; set; }

        public string? phone { get; set; }

        public string? role { get; set; }
    }
}