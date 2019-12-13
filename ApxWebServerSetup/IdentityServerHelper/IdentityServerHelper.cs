namespace IdentityServerHelper
{
    using IdentityModel.Client;
    using Newtonsoft.Json;
    using System;
    using System.Collections.Generic;
    using System.Net;
    using System.Net.Http;
    using System.Security.Cryptography.X509Certificates;
    using System.Text;

    public class Clients
    {
        [JsonProperty("totalCount")]
        public int TotalCount { get; set; }

        [JsonProperty("pageSize")]
        public int PageSize { get; set; }

        [JsonProperty("Clients")]
        public List<Client> ClientList { get; set; }
    }
    public class Client
    {
        [JsonProperty("Id")] public int Id { get; set; }
        [JsonProperty("Enabled")] public bool Enabled { get; set; }
        [JsonProperty("ClientId")] public string ClientId { get; set; }
        [JsonProperty("ProtocolType")] public string ProtocolType { get; set; }
        [JsonProperty("RequireClientSecret")] public bool RequireClientSecret { get; set; }
        [JsonProperty("ClientName")] public string ClientName { get; set; }
        [JsonProperty("Description")] public string Description { get; set; }
        [JsonProperty("ClientUri")] public string ClientUri { get; set; }
        [JsonProperty("LogoUri")] public string LogoUri { get; set; }
        [JsonProperty("RequireConsent")] public bool RequireConsent { get; set; }
        [JsonProperty("AllowRememberConsent")] public bool AllowRememberConsent { get; set; }
        [JsonProperty("AlwaysIncludeUserClaimsInIdToken")] public bool AlwaysIncludeUserClaimsInIdToken { get; set; }
        [JsonProperty("RequirePkce")] public bool RequirePkce { get; set; }
        [JsonProperty("AllowPlainTextPkce")] public bool AllowPlainTextPkce { get; set; }
        [JsonProperty("AllowAccessTokensViaBrowser")] public bool AllowAccessTokensViaBrowser { get; set; }
        [JsonProperty("FrontChannelLogoutUri")] public string FrontChannelLogoutUri { get; set; }
        [JsonProperty("FrontChannelLogoutSessionRequired")] public bool FrontChannelLogoutSessionRequired { get; set; }
        [JsonProperty("BackChannelLogoutUri")] public string BackChannelLogoutUri { get; set; }
        [JsonProperty("BackChannelLogoutSessionRequired")] public bool BackChannelLogoutSessionRequired { get; set; }
        [JsonProperty("AllowOfflineAccess")] public bool AllowOfflineAccess { get; set; }
        [JsonProperty("IdentityTokenLifetime")] public int IdentityTokenLifetime { get; set; }
        [JsonProperty("AccessTokenLifetime")] public int AccessTokenLifetime { get; set; }
        [JsonProperty("AuthorizationCodeLifetime")] public int AuthorizationCodeLifetime { get; set; }
        [JsonProperty("ConsentLifetime")] public int? ConsentLifetime { get; set; }
        [JsonProperty("AbsoluteRefreshTokenLifetime")] public int AbsoluteRefreshTokenLifetime { get; set; }
        [JsonProperty("SlidingRefreshTokenLifetime")] public int SlidingRefreshTokenLifetime { get; set; }
        [JsonProperty("RefreshTokenUsage")] public int RefreshTokenUsage { get; set; }
        [JsonProperty("UpdateAccessTokenClaimsOnRefresh")] public bool UpdateAccessTokenClaimsOnRefresh { get; set; }
        [JsonProperty("RefreshTokenExpiration")] public int RefreshTokenExpiration { get; set; }
        [JsonProperty("AccessTokenType")] public int AccessTokenType { get; set; }
        [JsonProperty("EnableLocalLogin")] public bool EnableLocalLogin { get; set; }
        [JsonProperty("IncludeJwtId")] public bool IncludeJwtId { get; set; }
        [JsonProperty("AlwaysSendClientClaims")] public bool AlwaysSendClientClaims { get; set; }
        [JsonProperty("ClientClaimsPrefix")] public string ClientClaimsPrefix { get; set; }
        [JsonProperty("PairWiseSubjectSalt")] public string PairWiseSubjectSalt { get; set; }
        [JsonProperty("Created")] public DateTime Created { get; set; }
        [JsonProperty("Updated")] public DateTime? Updated { get; set; }
        [JsonProperty("LastAccessed")] public DateTime? LastAccessed { get; set; }
        [JsonProperty("UserSsoLifetime")] public int? UserSsoLifetime { get; set; }
        [JsonProperty("UserCodeType")] public string UserCodeType { get; set; }
        [JsonProperty("DeviceCodeLifetime")] public int DeviceCodeLifetime { get; set; }
        [JsonProperty("NonEditable")] public bool NonEditable { get; set; }

        [JsonProperty("postLogoutRedirectUris")] public List<string> postLogoutRedirectUris { get; set; }
        [JsonProperty("identityProviderRestrictions")] public List<string> identityProviderRestrictions { get; set; }
        [JsonProperty("redirectUris")] public List<string> redirectUris { get; set; }
        [JsonProperty("allowedCorsOrigins")] public List<string> allowedCorsOrigins { get; set; }
        [JsonProperty("allowedGrantTypes")] public List<string> allowedGrantTypes { get; set; }
        [JsonProperty("allowedScopes")] public List<string> allowedScopes { get; set; }
        [JsonProperty("claims")] public List<Claim> claims { get; set; }
        [JsonProperty("clientSecrets")] public List<clientSecret> clientSecrets { get; set; }
        [JsonProperty("properties")] public List<string> properties { get; set; }
    }

    public class Claim
    {
        [JsonProperty("Id")] public int Id { get; set; }
        [JsonProperty("Type")] public string Type { get; set; }
        [JsonProperty("Value")] public string Value { get; set; }
    }

    public class clientSecret
    {
        [JsonProperty("Id")] public int Id { get; set; }
        [JsonProperty("Description")] public string Description { get; set; }
        [JsonProperty("Value")] public string Value { get; set; }
        [JsonProperty("Expiration")] public DateTime? Expiration { get; set; }
        [JsonProperty("Type")] public string Type { get; set; }
        [JsonProperty("Created")] public DateTime Created { get; set; }
    }
    public class IdentityServerHelper
    {
        private string authority;

        public IdentityServerHelper(string applicationServer)
        {
            this.authority = string.Format("http://{0}:5000", applicationServer);
        }

        public void InstallCertificate()
        {
            string requestUri = string.Format("{0}/.well-known/openid-configuration", authority);
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(requestUri);
            request.AllowAutoRedirect = false;
            request.ServerCertificateValidationCallback += (sender, cert, chain, sslPolicyErrors) => true;

            using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
            {
                X509Certificate2 certificate = new X509Certificate2(request.ServicePoint.Certificate);
                certificate.FriendlyName = "AdventIdentityServer";
                using (X509Store store = new X509Store(StoreName.Root, StoreLocation.LocalMachine))
                {
                    store.Open(OpenFlags.OpenExistingOnly | OpenFlags.ReadWrite);
                    store.Add(certificate);
                }
            }
        }

        public void RegisterApxWebClient(string webserverFqdn, string username, string password)
        {
            string accessToken = this.RequestToken(username, password);
            HttpClient httpclient = new HttpClient();
            httpclient.DefaultRequestHeaders.Add("Authorization", string.Format("Bearer {0}", accessToken));
            httpclient.DefaultRequestHeaders.Add("accept", "application/json");

            // get all clients
            string requestUri = string.Format("{0}/api/Clients", authority);
            var getTask = httpclient.GetAsync(requestUri);
            getTask.Wait();
            string getResult = getTask.Result.Content.ReadAsStringAsync().Result;
            var clients = JsonConvert.DeserializeObject<Clients>(getResult);

            // find asp.net client
            Client aspdotnetClient = null;
            foreach (Client client in clients.ClientList)
            {
                if (client.ClientId == "authcode.apxaspdotnetui")
                {
                    aspdotnetClient = client;
                    break;
                }
            }

            // set asp.net client's redirectUris and postLogoutRedirectUris
            if (aspdotnetClient != null)
            {
                aspdotnetClient.redirectUris = new List<string> {
                        string.Format("http://{0}/apxlogin/identityserverlogin.aspx", webserverFqdn),
                        string.Format("https://{0}/apxlogin/identityserverlogin.aspx", webserverFqdn),
                        string.Format("http://{0}/apx/identityserverlogin.aspx", webserverFqdn),
                        string.Format("https://{0}/apx/identityserverlogin.aspx", webserverFqdn),
                };
                aspdotnetClient.postLogoutRedirectUris = new List<string> {
                        string.Format("http://{0}/apxlogin/IdentityServerSendAuthnRequest.aspx", webserverFqdn),
                        string.Format("https://{0}/apxlogin/IdentityServerSendAuthnRequest.aspx", webserverFqdn),
                        string.Format("http://{0}/apx/IdentityServerSendAuthnRequest.aspx?linkfield2=showlogindialog", webserverFqdn),
                        string.Format("https://{0}/apx/IdentityServerSendAuthnRequest.aspx?linkfield2=showlogindialog", webserverFqdn)
                };
            }

            string requestBody = JsonConvert.SerializeObject(aspdotnetClient);
            HttpContent httpcontent = new StringContent(requestBody, Encoding.UTF8, "application/json");
            var putTask = httpclient.PutAsync(requestUri, httpcontent);
            putTask.Wait();
            string putResult = putTask.Result.Content.ReadAsStringAsync().Result;
        }

        private string RequestToken(string username, string password)
        {
            HttpClient httpClient = new HttpClient();
            var discoTask = httpClient.GetDiscoveryDocumentAsync(new DiscoveryDocumentRequest
            {
                Address = this.authority,
                Policy = { RequireHttps = false }
            });

            discoTask.Wait();
            var disco = discoTask.Result;

            var task = httpClient.RequestPasswordTokenAsync(new PasswordTokenRequest()
            {
                Address = disco.TokenEndpoint,
                ClientId = "ro.identity_admin_api",
                Scope = "IdentityServerAdministrator",
                UserName = username,
                Password = password
            });

            task.Wait();
            var token = task.Result.AccessToken;
            return token;
        }
    }
}
