
namespace AdvOidcSample
{
    using IdentityModel.Client;
    using IdentityModel.OidcClient;
    using Newtonsoft.Json;
    using System;
    using System.Diagnostics;
    using System.Net;
    using System.Net.Http;
    using System.Net.Sockets;

    public class AuthenticationConfiguration
    {
        public string Issuer;
        public bool EnabledTokenBasedAuthentication;
    }

    public class ApxOidcClient
    {
        public string BaseAddress
        {
            get;
            set;
        }

        public string ClientId
        {
            get;
            set;
        }

        public string ClientSecret
        {
            get;
            set;
        }

        public string Scope
        {
            get;
            set;
        }

        private DiscoveryDocumentResponse _disco;

        private DiscoveryDocumentResponse Disco
        {
            get
            {
                if (this._disco == null)
                {
                    this._disco = this.GetDiscoveryDocument();
                }

                return this._disco;
            }
        }

        private string GetIssuer()
        {
            Uri requestUri = null;
            Uri baseUri = new Uri(this.BaseAddress);
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
                return config.Issuer;
            }

            return null;
        }

        private DiscoveryDocumentResponse GetDiscoveryDocument()
        {
            string issuer = this.GetIssuer();

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
        /// Login through Password flow which requires providing username and password to client app.
        /// </summary>
        /// <param name="username"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        public TokenResponse PasswordLogin(string username, string password)
        {
            this.BypassSelfSignedCertificateValidationError();

            var client = new HttpClient();
            var response = client.RequestPasswordTokenAsync(new PasswordTokenRequest
            {
                Address = this.Disco.TokenEndpoint,
                ClientId = this.ClientId,
                ClientSecret = this.ClientSecret,
                Scope = this.Scope,
                UserName = username,
                Password = password
            });

            response.Wait();
            return response.Result;
        }

        /// <summary>
        /// Login through Windows Authentication Flow which supports Windows-Integrated users.
        /// </summary>
        /// <returns></returns>
        public TokenResponse WindowsLogin()
        {
            this.BypassSelfSignedCertificateValidationError();

            var handler = new HttpClientHandler();
            handler.UseDefaultCredentials = true;

            var client = new HttpClient(handler);
            var response = client.RequestTokenAsync(new TokenRequest
            {
                Address = this.Disco.TokenEndpoint,
                ClientId = this.ClientId,
                ClientSecret = this.ClientSecret,
                Parameters = {
                    { "scope", this.Scope}
                },
                GrantType = "WindowsAuth",
            });

            response.Wait();
            var result = response.Result;
            this.Print(result);
            return result;
        }

        public TokenResponse RefreshToken(string refreshToken)
        {
            this.BypassSelfSignedCertificateValidationError();

            var client = new HttpClient();
            var response = client.RequestRefreshTokenAsync(new  RefreshTokenRequest
            {
                Address = this.Disco.TokenEndpoint,
                ClientId = this.ClientId,
                ClientSecret = this.ClientSecret,
                RefreshToken = refreshToken                
            });

            response.Wait();
            var result = response.Result;
            this.Print(result);
            return result;
        }

        /// <summary>
        /// Login through Authorization Code Flow which requires user interaction to provide username and password to Identity Server.
        /// </summary>
        /// <returns></returns>
        public LoginResult AuthorizationCodeLogin()
        {
            this.BypassSelfSignedCertificateValidationError();

            string redirectUri = this.GenerateRedirectUri();

            var lisener = new HttpListener();
            lisener.Prefixes.Add(redirectUri);
            lisener.Start();

            // You must make sure the client and redirect uri are registered to Identity Server, otherwise you will see "unauthorized client" error.
            var options = new OidcClientOptions
            {
                Authority = this.Disco.Issuer,
                ClientId = this.ClientId,
                Scope = this.Scope,
                RedirectUri = redirectUri, 
                Flow = OidcClientOptions.AuthenticationFlow.AuthorizationCode,
                ResponseMode = OidcClientOptions.AuthorizeResponseMode.Redirect
            };

            var client = new OidcClient(options);
            var state = client.PrepareLoginAsync();
            state.Wait();

            Process browser = Process.Start(state.Result.StartUrl);

            var context = lisener.GetContextAsync();
            context.Wait();

            lisener.Stop();

            var data = this.GetCallBackData(context.Result.Request);
            var response = client.ProcessResponseAsync(data, state.Result);
            response.Wait();

            var result = response.Result;
            this.Print(result);

            return result;
        }

        private string GenerateRedirectUri()
        {
            var listener = new TcpListener(IPAddress.Loopback, 0);
            listener.Start();
            var port = ((IPEndPoint)listener.LocalEndpoint).Port;
            listener.Stop();

            string redirectUri = string.Format("http://127.0.0.1:{0}/", port);
            return redirectUri;
        }

        private string GetCallBackData(HttpListenerRequest request)
        {
            if (request == null)
            {
                throw new NullReferenceException("Object request is null.");
            }
            else if (request.HasEntityBody)
            { // responsemode = formpost
                using (var reader = new System.IO.StreamReader(request.InputStream, request.ContentEncoding))
                {
                    return reader.ReadToEnd();
                }
            }
            else if (request.QueryString != null && request.QueryString.Count > 0)
            { //responsemode = redirect
                string data = null;
                foreach (string key in request.QueryString.AllKeys)
                {
                    data += data == null ?
                        string.Format("{0}={1}", key, request.QueryString[key]) :
                        string.Format("&{0}={1}", key, request.QueryString[key]);
                }

                return data;
            }
            else
            {
                return null;
            }
        }

        private void BypassSelfSignedCertificateValidationError()
        {
            // If you are using self-signed cerificate, you may need to bypass certificate validation errors
            // or create your logic to validate certificate.
            ServicePointManager.ServerCertificateValidationCallback += (sender, cert, chain, sslPolicyErrors) => true;
        }

        private void Print(TokenResponse reponse)
        {
            Console.WriteLine("==========Start Printing Token==========");
            if (reponse == null)
            {
                Console.WriteLine("Object \"reponse\" is null.");
            }
            else if (reponse.IsError)
            {
                Console.WriteLine(reponse.ErrorDescription);
            }
            else
            {
                Console.WriteLine("IdentityToken");
                Console.WriteLine(reponse.IdentityToken);
                Console.WriteLine("========================================================");
                Console.WriteLine("AccessToken");
                Console.WriteLine(reponse.AccessToken);
                Console.WriteLine("========================================================");
                Console.WriteLine("RefreshToken");
                Console.WriteLine(reponse.RefreshToken);
            }

            Console.WriteLine("==========End Printing Token==========");
            //Console.WriteLine("Press Enter to continue.");
            //Console.ReadLine();
        }

        private void Print(LoginResult result)
        {
            Console.WriteLine("=======================Begin=========================");
            if (result == null)
            {
                Console.WriteLine("Object \"result\" is null.");
            }
            else if (result.IsError)
            {
                Console.WriteLine(result.Error);
            }
            else
            {
                Console.WriteLine("Id_Token:", result.IdentityToken);
                Console.WriteLine(result.IdentityToken);
                Console.WriteLine("================================================");
                Console.WriteLine("Refresh_Token:", result.RefreshToken);
                Console.WriteLine(result.RefreshToken);
                Console.WriteLine("================================================");
                Console.WriteLine("Access_Token:", result.AccessToken);
                Console.WriteLine(result.AccessToken);
            }

            Console.WriteLine("=====================End===========================");
            //Console.WriteLine("Press Enter to continue.");
            //Console.ReadLine();
        }
    }
}
