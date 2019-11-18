
namespace AdvOidcSample
{
    using IdentityModel.Client;
    using IdentityModel.OidcClient;
    using System;
    using System.Diagnostics;
    using System.Net;
    using System.Net.Http;
    using System.Net.Sockets;

    public class AdvIdentityClient
    {
        public string Issuer
        {
            get
            {
                if (this.discovery == null)
                {
                    return null;
                }

                return this.discovery.Issuer;
            }
            set
            {
                this.discovery = this.GetDiscoveryDocument(value);
            }
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

        private DiscoveryDocumentResponse discovery;

        private DiscoveryDocumentResponse GetDiscoveryDocument(string address)
        {
            if (string.IsNullOrEmpty(address))
            {
                throw new ArgumentNullException("address can't be null or empty.");
            }

            this.BypassSelfSignedCertificateValidationError();

            var client = new HttpClient();
            var response = client.GetDiscoveryDocumentAsync(new DiscoveryDocumentRequest
            {
                Address = address,
                Policy = { RequireHttps = false }
            });
            response.Wait();
            if (response.IsFaulted)
            {
                throw response.Exception;
            }

            return response.Result;
        }

        /// <summary>
        /// Login with APX Username and Password
        /// </summary>
        /// <param name="username"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        public TokenResponse Login(string username, string password)
        {
            this.BypassSelfSignedCertificateValidationError();

            var client = new HttpClient();
            var response = client.RequestPasswordTokenAsync(new PasswordTokenRequest
            {
                Address = this.discovery.TokenEndpoint,
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
        /// Login with Windows-NT User
        /// </summary>
        /// <returns></returns>
        public TokenResponse Login()
        {
            this.BypassSelfSignedCertificateValidationError();

            var handler = new HttpClientHandler();
            handler.UseDefaultCredentials = true;

            var client = new HttpClient(handler);
            var response = client.RequestTokenAsync(new TokenRequest
            {
                Address = this.discovery.TokenEndpoint,
                ClientId = this.ClientId,
                ClientSecret = this.ClientSecret,
                Parameters = {
                    { "scope", this.Scope}
                },
                GrantType = "WindowsAuth",
            });

            response.Wait();
            return response.Result;
        }

        public TokenResponse RefreshToken(string refreshToken)
        {
            this.BypassSelfSignedCertificateValidationError();

            var client = new HttpClient();
            var response = client.RequestRefreshTokenAsync(new  RefreshTokenRequest
            {
                Address = this.discovery.TokenEndpoint,
                ClientId = this.ClientId,
                ClientSecret = this.ClientSecret,
                RefreshToken = refreshToken                
            });

            response.Wait();
            return response.Result;
        }

        /// <summary>
        /// Signin via Identity Server Login Page
        /// </summary>
        /// <returns></returns>
        public LoginResult Signin()
        {
            this.BypassSelfSignedCertificateValidationError();

            string redirectUri = this.GenerateRedirectUri();

            var lisener = new HttpListener();
            lisener.Prefixes.Add(redirectUri);
            lisener.Start();

            var options = new OidcClientOptions
            {
                Authority = this.Issuer,
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
            var result = client.ProcessResponseAsync(data, state.Result);
            result.Wait();

            return result.Result;
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
            // bypass self-signed Certificate error
            ServicePointManager.ServerCertificateValidationCallback += (sender, cert, chain, sslPolicyErrors) => true;
        }
    }
}
