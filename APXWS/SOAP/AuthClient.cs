namespace Advent.ApxSoap
{
    using IdentityModel.Client;
    using Newtonsoft.Json;
    using System;
    using System.Net;
    using System.Net.Http;
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
                TokenResponse token = this.OidcLogin(username, password);
                apxws = this.CreateApxWS(token.AccessToken);
            }
            else
            {
                AuthWS.AuthenticateWS authWS = this.ApxLogin(username, password);
                apxws = this.CreateApxWS(authWS);
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
                TokenResponse token = this.OidcLogin();
                apxws = this.CreateApxWS(token.AccessToken);
            }
            else
            {
                AuthWS.AuthenticateWS authWS = this.ApxLogin();
                apxws = this.CreateApxWS(authWS);
            }

            return apxws;
        }

        /// <summary>
        /// Log in through Identity Server as Win NT User
        /// </summary>
        /// <returns></returns>
        private TokenResponse OidcLogin()
        {
            this.BypassSelfSignedCertificateValidationError();

            var handler = new HttpClientHandler();
            handler.UseDefaultCredentials = true;

            var client = new HttpClient(handler);
            var response = client.RequestTokenAsync(new TokenRequest
            {
                Address = this.disco.TokenEndpoint,
                ClientId = "ro.APXAPIClient",
                ClientSecret = "advs",
                Parameters = {
                    { "scope", "apxapi"}
                },
                GrantType = "WindowsAuth",
            });

            response.Wait();
            var result = response.Result;
            return result;
        }

        /// <summary>
        /// Log in through Identity Server as DB User
        /// </summary>
        /// <param name="username"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        private TokenResponse OidcLogin(string username, string password)
        {
            this.BypassSelfSignedCertificateValidationError();

            var client = new HttpClient();
            var response = client.RequestPasswordTokenAsync(new PasswordTokenRequest
            {
                Address = this.disco.TokenEndpoint,
                ClientId = "ro.APXAPIClient",
                ClientSecret = "advs",
                Scope = "apxapi",
                UserName = username,
                Password = password
            });

            response.Wait();
            return response.Result;
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
                var response = client.GetAsync(requestUri);
                response.Wait();
                var result = response.Result;
                result.EnsureSuccessStatusCode();
                var content = result.Content.ReadAsStringAsync().Result;
                var config = JsonConvert.DeserializeObject<AuthenticationConfiguration>(content);
                return config;
            }

            return null;
        }

        private void BypassSelfSignedCertificateValidationError()
        {
            // If you are using self-signed cerificate, you may need to bypass certificate validation errors
            // or create your logic to validate certificate.
            ServicePointManager.ServerCertificateValidationCallback += (sender, cert, chain, sslPolicyErrors) => true;
        }

        /// <summary>
        /// Get Identity Server Discovery Document
        /// </summary>
        /// <param name="issuer"></param>
        /// <returns></returns>
        private DiscoveryDocumentResponse GetDiscoveryDocument(string issuer)
        {
            this.BypassSelfSignedCertificateValidationError();

            var client = new HttpClient();
            var response = client.GetDiscoveryDocumentAsync(new DiscoveryDocumentRequest
            {
                Address = issuer,
                Policy = { RequireHttps = false }
            });

            response.Wait();
            return response.Result;
        }

        /// <summary>
        /// Create ApxWS instance with an access token
        /// </summary>
        /// <param name="accessToken"></param>
        /// <returns></returns>
        private ApxWS.ApxWS CreateApxWS(string accessToken)
        {
            ApxWS.ApxWS apxws = new ApxWS.ApxWS();
            apxws.AccessToken = accessToken;
            apxws.UseDefaultCredentials = false;
            this.ResolveServiceUrl(apxws);

            return apxws;
        }

        /// <summary>
        /// Log in through AuthFilter as Win NT User
        /// </summary>
        /// <returns></returns>
        private AuthWS.AuthenticateWS ApxLogin()
        {
            AuthWS.AuthenticateWS authWS = new AuthWS.AuthenticateWS();
            // Set UseDefaultCredentials to true for Windows integrated users.
            authWS.UseDefaultCredentials = true;
            this.ResolveServiceUrl(authWS);
            authWS.CookieContainer = new CookieContainer();
            
            return authWS;
        }

        /// <summary>
        /// Log in through AuthFilter as APX DB User
        /// </summary>
        /// <param name="username"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        private AuthWS.AuthenticateWS ApxLogin(string username, string password)
        {
            AuthWS.AuthenticateWS authWS = new AuthWS.AuthenticateWS();
            // Set UseDefaultCredentials to false for APX users.
            authWS.UseDefaultCredentials = false;
            this.ResolveServiceUrl(authWS); // this must be called before setting cookie Uri

            authWS.CookieContainer = new CookieContainer();
            Uri cookieUri = new Uri((new Uri(authWS.Url)).GetLeftPart(UriPartial.Authority));
            authWS.CookieContainer.Add(cookieUri, new Cookie("LoginName", username));
            authWS.CookieContainer.Add(cookieUri, new Cookie("Password", password));
            
            return authWS;
        }

        /// <summary>
        /// Create an ApxWS instance with cookies
        /// </summary>
        /// <param name="cookie"></param>
        /// <param name="UseDefaultCredentials"></param>
        /// <returns></returns>
        private ApxWS.ApxWS CreateApxWS(AuthWS.AuthenticateWS authWS)
        {
            ApxWS.ApxWS apxws = null;
            if (authWS.Login())
            {
                apxws = new ApxWS.ApxWS();
                apxws.UseDefaultCredentials = authWS.UseDefaultCredentials;
                apxws.CookieContainer = authWS.CookieContainer;
                this.ResolveServiceUrl(apxws);
            }
            else
            {
                throw new Exception("Login Falis.");
            }

            return apxws;
        }

        private void ResolveServiceUrl(SoapHttpClientProtocol service)
        {
            // replace authority with apxWebServerUrl
            Uri oldUri = new Uri(service.Url);
            Uri newUri = new Uri(this.apxWebServerUrl);
            service.Url = service.Url.Replace(oldUri.GetLeftPart(UriPartial.Authority), newUri.GetLeftPart(UriPartial.Authority));

            // Url for Windows NT user is like "../apx/..", while url for non-Windows NT user is like "../apxlogin/.."
            if (service.UseDefaultCredentials)
            {
                service.Url = service.Url.ToLower().Replace("/apxlogin/", "/apx/");
            }
            else
            {
                service.Url = service.Url.ToLower().Replace("/apx/", "/apxlogin/");
            }
        }
    }
}

