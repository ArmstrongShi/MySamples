
namespace AdvOidcSample
{
    using IdentityModel.Client;
    using System.Net.Http;
    using System.Threading.Tasks;

    /// <summary>
    /// This class is to show example to connect Identity Server to request token
    /// </summary>
    public class AdvOidcClient
    {
        private DiscoveryDocumentResponse _discovery;
        public AdvOidcClient(string idsBaseUrl)
        {
            this._discovery = this.GetDiscoveryDocument(idsBaseUrl);
        }

        private DiscoveryDocumentResponse GetDiscoveryDocument(string baseAddress)
        {
            var client = new HttpClient();
            var response = Task.Run(()=>client.GetDiscoveryDocumentAsync(new DiscoveryDocumentRequest
            {
                Address = baseAddress,
                Policy = { RequireHttps = false }
            })).Result;

            return response;
            
        }

        /// <summary>
        /// Log in as specified login name and password
        /// </summary>
        /// <param name="username"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        public TokenResponse Login(string username, string password)
        {
            var client = new HttpClient();
            var response = Task.Run(()=>client.RequestPasswordTokenAsync(new PasswordTokenRequest
            {
                Address = this._discovery.TokenEndpoint,
                ClientId = "ro.apxapiclient",
                ClientSecret = "advs",
                Scope = "apxapi",
                UserName = username,
                Password = password
            })).Result;

            return response;
        }

        /// <summary>
        /// Log in as current Windows user.
        /// </summary>
        /// <returns></returns>
        public TokenResponse Login()
        {
            var handler = new HttpClientHandler();
            handler.UseDefaultCredentials = true;

            var client = new HttpClient(handler);
            var response = Task.Run(()=>client.RequestTokenAsync(new TokenRequest
            {
                Address = this._discovery.TokenEndpoint,
                ClientId = "ro.apxapiclient",
                ClientSecret = "advs",
                Parameters = { { "scope", "apxapi" } },
                GrantType = "WindowsAuth",
            })).Result;

            return response;
        }
    }
}
