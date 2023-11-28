using GoogleApiAccessApplication.Common;
using GoogleApiAccessApplication.Models;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Diagnostics;

namespace GoogleApiAccessApplication.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private ServerResponse? oAuthDetails;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
            GoogleApiHelper.SetServerResponseDetails(GetClientDetails().Result);
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

        public async Task<ServerResponse> GetClientDetails()
        {
            using (HttpClient cl = new HttpClient())
            {
                string url = "https://localhost:44347/api/SSO/GetGoogleSSODetailsByDomainName/";
                string domainName = "Google";
                oAuthDetails = JsonConvert.DeserializeObject<ServerResponse>(await cl.GetStringAsync(url + domainName));
            }
            return oAuthDetails;
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
                                HttpResponseMessage response = client.PostAsync("https://localhost:44347/api/ExternalLogin/Google/oAuthGoogleToken", httpContent).Result;
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

    public class ServerResponse
    {
        public int? Id { get; set; }

        public string? DomainName { get; set; }

        public string? DomainCode { get; set; }

        public string? ClientId { get; set; }

        public string? ClientSecretCode { get; set; }

        public string? GrantType { get; set; }

        public string? RedirectUrl { get; set; }

        public string? TokenUrl { get; set; }

        public string? GraphUrl { get; set; }

        public bool? ActiveYn { get; set; }

        public string? Oauthurl { get; set; }

        public string? Scopes { get; set; }

        public string? ApplicationName { get; set; }
    }
}