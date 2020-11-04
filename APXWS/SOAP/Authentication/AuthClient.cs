namespace Advent.ApxSoap.Authentication
{
    using IdentityModel.Client;
    using Newtonsoft.Json;
    using System;
    using System.Net;
    using System.Net.Http;
    using System.Threading.Tasks;
    using System.Web.Services.Protocols;

    public class AuthenticationConfiguration
    {
        public string Issuer;
        public bool EnabledTokenBasedAuthentication;
    }

    public class AuthClient
    {
        private AuthenticationConfiguration authConfig;
        private DiscoveryDocumentResponse disco;
        private TokenResponse token;
        private string apxWebServerUrl;

        public AuthClient(string apxWebServerUrl)
        {
            this.apxWebServerUrl = apxWebServerUrl;
            this.authConfig = this.GetApxAuthenticationConfiguration(apxWebServerUrl);
            this.disco = this.GetDiscoveryDocument(this.authConfig.Issuer);
        }

        /// <summary>
        /// Log in as APX DB User
        /// </summary>
        /// <param name="username"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        public ApxWS.ApxWS Login(string username, string password)
        {
            ApxWS.ApxWS apxws = null;
            if (this.authConfig.EnabledTokenBasedAuthentication)
            {
                this.token = this.RequestToken(username, password);
                apxws = this.CreateApxWS(token.AccessToken, this.apxWebServerUrl);
            }
            else
            {
                throw new Exception("Token Based Authentication is not enabled.");
            }

            return apxws;
        }

        /// <summary>
        /// Log in as APX Win NT user
        /// </summary>
        /// <returns></returns>
        public ApxWS.ApxWS Login()
        {
            ApxWS.ApxWS apxws = null;
            if (this.authConfig.EnabledTokenBasedAuthentication)
            {
                this.token = this.RequestToken();
                apxws = this.CreateApxWS(token.AccessToken, this.apxWebServerUrl);
            }
            else
            {
                throw new Exception("Token Based Authentication is not enabled.");
            }

            return apxws;
        }

        /// <summary>
        /// Log out from Apx and end user session
        /// </summary>
        /// <returns></returns>
        public void Logout()
        {
            HttpClient client = new HttpClient();
            client.DefaultRequestHeaders.Add("Authorization", $"Bearer {this.token.AccessToken}");
            string requestUrl = $"{this.apxWebServerUrl}/apxlogin/api/odata/EndSession";
            var response = Task.Run(() => client.GetAsync(requestUrl)).Result;
            response.EnsureSuccessStatusCode();
        }

        /// <summary>
        /// Log in through Identity Server as Win NT User
        /// </summary>
        /// <returns></returns>
        private TokenResponse RequestToken()
        {
            var handler = new HttpClientHandler();
            handler.UseDefaultCredentials = true;

            var client = new HttpClient(handler);
            var token = Task.Run(()=>client.RequestTokenAsync(new TokenRequest
            {
                Address = this.disco.TokenEndpoint,
                ClientId = "ro.APXAPIClient",
                ClientSecret = "advs",
                Parameters = {
                    { "scope", "apxapi"}
                },
                GrantType = "WindowsAuth",
            })).Result;

            if (token.IsError)
            {
                throw new Exception(token.ErrorDescription);
            }

            return token;
        }

        /// <summary>
        /// Log in through Identity Server as DB User
        /// </summary>
        /// <param name="username"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        private TokenResponse RequestToken(string username, string password)
        {
            var client = new HttpClient();
            var token = Task.Run(() => client.RequestPasswordTokenAsync(new PasswordTokenRequest
            {
                Address = this.disco.TokenEndpoint,
                ClientId = "ro.APXAPIClient",
                ClientSecret = "advs",
                Scope = "apxapi",
                UserName = username,
                Password = password
            })).Result;

            if (token.IsError)
            {
                throw new Exception(token.ErrorDescription);
            }

            return token;
        }

        /// <summary>
        /// Get APX authentication configuration
        /// </summary>
        /// <param name="apxWebServerUrl"></param>
        /// <returns></returns>
        private AuthenticationConfiguration GetApxAuthenticationConfiguration(string apxWebServerUrl)
        {
            Uri requestUri = null;
            Uri baseUri = new Uri(apxWebServerUrl);
            // You must get the issuer from this APX well-known API first, which does not require login
            string relativeUri = "apxlogin/api/well-known/authentication-configuration";
            if (Uri.TryCreate(baseUri, relativeUri, out requestUri))
            {
                var client = new HttpClient();
                var result = Task.Run(() => client.GetAsync(requestUri)).Result;
                result.EnsureSuccessStatusCode();
                var content = result.Content.ReadAsStringAsync().Result;
                var config = JsonConvert.DeserializeObject<AuthenticationConfiguration>(content);
                return config;
            }

            return null;
        }

        /// <summary>
        /// Get Identity Server Discovery Document
        /// </summary>
        /// <param name="issuer"></param>
        /// <returns></returns>
        private DiscoveryDocumentResponse GetDiscoveryDocument(string issuer)
        {
            var client = new HttpClient();
            var result = Task.Run(()=>client.GetDiscoveryDocumentAsync(new DiscoveryDocumentRequest
            {
                Address = issuer,
                Policy = { RequireHttps = false }
            })).Result;

            return result;
        }

        /// <summary>
        /// Create ApxWS instance with an access token
        /// </summary>
        /// <param name="accessToken"></param>
        /// <returns></returns>
        private ApxWS.ApxWS CreateApxWS(string accessToken,string apxwebUrl)
        {
            ApxWS.ApxWS apxws = new ApxWS.ApxWS();
            apxws.AccessToken = accessToken;
            // Now, you can only use apxlogin virtual directory for API access
            apxws.UseDefaultCredentials = false;
            apxws.Url = $"{apxwebUrl}/apxlogin/services/V2/ApxWS.asmx";
            
            return apxws;
        }
    }
}

