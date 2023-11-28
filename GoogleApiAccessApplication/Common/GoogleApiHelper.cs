using GoogleApiAccessApplication.Controllers;
using Newtonsoft.Json;
using System.Text;

namespace GoogleApiAccessApplication.Common
{
    public class GoogleApiHelper
    {
        //public static string ApplicationName = "NetCoreAccessGooglee";
        //public static string ClientId = "490036622696-erkssf2726h8e9vu43it5lh9b56lqibh.apps.googleusercontent.com";
        //public static string ClientSecret = "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";
        //public static string RedirectUri = "https://localhost:7244/Home/OauthCallback";
        //public static string OauthUri = "https://accounts.google.com/o/oauth2/auth?";
        //public static string TokenUri = "https://accounts.google.com/o/oauth2/token";
        //public static string Scopes = "https://www.googleapis.com/auth/userinfo.email";
        public static string ApplicationName;
        public static string ClientId;
        public static string ClientSecret;
        public static string RedirectUri;
        public static string OauthUri;
        public static string TokenUri;
        public static string Scopes;

        public static void SetServerResponseDetails(ServerResponse serverResponse)
        {
            ApplicationName = serverResponse.ApplicationName;
            ClientId = serverResponse.ClientId;
            ClientSecret = serverResponse.ClientSecretCode;
            RedirectUri = serverResponse.RedirectUrl;
            OauthUri = serverResponse.Oauthurl;
            TokenUri = serverResponse.TokenUrl;
            Scopes = serverResponse.Scopes;
        }
        public static string GetOauthUri(string extraParam)
        {        
            StringBuilder sbUri = new StringBuilder(OauthUri);
            sbUri.Append("client_id="+ClientId);
            sbUri.Append("&redirect_uri=" + RedirectUri);
            sbUri.Append("&response_type=" + "code");
            sbUri.Append("&scope=" + Scopes);
            sbUri.Append("&access_type=" + "offline");
            sbUri.Append("&state=" + extraParam);
            sbUri.Append("&approval_prompt=" + "force");
            return sbUri.ToString();
        }

        public static async Task<GoogleToken> GetTokenByCode(string code)
        {
            GoogleToken token = null;
            var postData = new
            {
                code = code,
                client_id = ClientId,
                client_secret = ClientSecret,
                redirect_uri = RedirectUri,
                grant_type = "authorization_code"
            };

            using(var httpClient = new HttpClient())
            {
                StringContent content = new StringContent(JsonConvert.SerializeObject(postData),Encoding.UTF8,"application/json");

                using(var response = await httpClient.PostAsync(TokenUri, content)) 
                { 
                    if(response.StatusCode == System.Net.HttpStatusCode.OK)
                    {
                        string responseString  = await response.Content.ReadAsStringAsync();

                        token = JsonConvert.DeserializeObject<GoogleToken>(responseString);
                    }
                }
            }

            return token;
        }
    }
}
