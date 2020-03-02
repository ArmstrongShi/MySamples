namespace Advent.ApxSetup
{
    using IdentityModel.Client;
    using Newtonsoft.Json;
    using System;
    using System.Collections.Generic;
    using System.Linq;
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

    public class AuthenticationConfiguration
    {
        public string Issuer;
        public bool EnabledTokenBasedAuthentication;
    }

    public static class IdentityServerHelper
    {
        public static void InstallIdsAppServerCertificate(string appServerFqdn)
        {
            string requestUri = "https://" + appServerFqdn + ":5001/";
            InstallIdsCertificate(requestUri);
        }

        public static void RegisterNewUIRedirectUris(string apiServerUrl, string idsAdminUsername, string idsAdminPassword, string newUIserverFqdn)
        {
            // New UI installer only knows APX API Server Url (aka. Apx Web Server Url)
            // So New UI installer needs to call ./apxlogin/api/well-known/authentication-configuration to know APX's idsAuthority first.
            string idsProxyUrl = IdentityServerHelper.GetIdsAuthorityFromApx(apiServerUrl);

            InstallIdsCertificate(idsProxyUrl);
            // Request access token with Ids Admin Api username and password.
            string token = RequestPasswordToken(idsProxyUrl, idsAdminUsername, idsAdminPassword);
            Client client = new Client()
            {
                ClientId = "authcode.apxui",
                redirectUris = new System.Collections.Generic.List<string>() {
                    // signin callback uri
                    string.Format("http://{0}/APXUILogin/#/signin-callback", newUIserverFqdn),
                    string.Format("http://{0}/APXUI/#/signin-callback", newUIserverFqdn),
                    string.Format("https://{0}/APXUILogin/#/signin-callback", newUIserverFqdn),
                    string.Format("https://{0}/APXUI/#/signin-callback", newUIserverFqdn),
                    // silent refresh callback uri
                    string.Format("http://{0}/APXUILogin/silent-refresh.html", newUIserverFqdn),
                    string.Format("http://{0}/APXUI/silent-refresh.html", newUIserverFqdn),
                    string.Format("https://{0}/APXUILogin/silent-refresh.html", newUIserverFqdn),
                    string.Format("https://{0}/APXUI/silent-refresh.html", newUIserverFqdn)
                },
                postLogoutRedirectUris = new System.Collections.Generic.List<string>() {
                    // signout callback uri
                    string.Format("http://{0}/APXUILogin/#/signout-callback", newUIserverFqdn),
                    string.Format("http://{0}/APXUI/#/signout-callback", newUIserverFqdn),
                    string.Format("https://{0}/APXUILogin/#/signout-callback", newUIserverFqdn),
                    string.Format("https://{0}/APXUI/#/signout-callback", newUIserverFqdn)
                },
                allowedCorsOrigins = new List<string>() {
                    string.Format("http://{0}", newUIserverFqdn),
                    string.Format("https://{0}", newUIserverFqdn)
                }
            };

            // Call Admin API to register new redirect Uris.
            RegisterClientRedirectUris(idsProxyUrl, token, client);
        }

        public static void RegisterApxWebRedirectUris(string idsProxyUrl, string idsAdminUsername, string idsAdminPassword, string webServerFqdn)
        {
            InstallIdsCertificate(idsProxyUrl);
            string token = RequestPasswordToken(idsProxyUrl, idsAdminUsername, idsAdminPassword);
            Client client = new Client()
            {
                ClientId = "authcode.apxaspdotnetui",
                redirectUris = new System.Collections.Generic.List<string>() {
                    string.Format("http://{0}/apxlogin/identityserverlogin.aspx", webServerFqdn),
                    string.Format("http://{0}/apx/identityserverlogin.aspx", webServerFqdn),
                    string.Format("https://{0}/apxlogin/identityserverlogin.aspx", webServerFqdn),
                    string.Format("https://{0}/apx/identityserverlogin.aspx", webServerFqdn)
                },
                postLogoutRedirectUris = new System.Collections.Generic.List<string>() {
                    string.Format("http://{0}/apxlogin/IdentityServerSendAuthnRequest.aspx", webServerFqdn),
                    string.Format("http://{0}/apx/IdentityServerSendAuthnRequest.aspx?linkfield2=showlogindialog", webServerFqdn),
                    string.Format("https://{0}/apxlogin/IdentityServerSendAuthnRequest.aspx", webServerFqdn),
                    string.Format("https://{0}/apx/IdentityServerSendAuthnRequest.aspx?linkfield2=showlogindialog", webServerFqdn)
                }
            };

            RegisterClientRedirectUris(idsProxyUrl, token, client);
        }

        private static void RegisterClientRedirectUris(string idsAuthority, string accessToken, Client client)
        {
            HttpClient httpclient = new HttpClient();
            httpclient.DefaultRequestHeaders.Add("Authorization", string.Format("Bearer {0}", accessToken));
            httpclient.DefaultRequestHeaders.Add("accept", "application/json");

            // get all clients
            string requestUri = string.Format("{0}/api/Clients", idsAuthority);
            var get = httpclient.GetAsync(requestUri);
            get.Wait();

            string result = get.Result.Content.ReadAsStringAsync().Result;
            var clients = JsonConvert.DeserializeObject<Clients>(get.Result.Content.ReadAsStringAsync().Result);
            Client c = clients.ClientList.First<Client>(x => x.ClientId == client.ClientId);
            if (clients.ClientList.First<Client>(x => x.ClientId == client.ClientId) != null)
            {
                //c.redirectUris = client.redirectUris;
                //c.postLogoutRedirectUris = client.postLogoutRedirectUris;

                if (c.redirectUris == null)
                {
                    c.redirectUris = new List<string>(client.redirectUris);
                }
                else
                {
                    // c.redirectUris.AddRange(client.redirectUris);
                    foreach (string uri in client.redirectUris)
                    {
                        if (!c.redirectUris.Any<string>(x => x == uri))
                        {
                            c.redirectUris.Add(uri);
                        }
                    }
                }

                if (c.postLogoutRedirectUris == null)
                {
                    c.postLogoutRedirectUris = new List<string>(client.postLogoutRedirectUris);
                }
                else
                {
                    // c.postLogoutRedirectUris.AddRange(client.postLogoutRedirectUris);
                    foreach (string uri in client.postLogoutRedirectUris)
                    {
                        if (!c.postLogoutRedirectUris.Any<string>(x => x == uri))
                        {
                            c.postLogoutRedirectUris.Add(uri);
                        }
                    }
                }

                if (c.allowedCorsOrigins == null)
                {
                    c.allowedCorsOrigins = new List<string>(client.allowedCorsOrigins);
                }
                else
                {
                    foreach (string uri in client.allowedCorsOrigins)
                    {
                        if (!c.allowedCorsOrigins.Any<string>(x => x == uri))
                        {
                            c.allowedCorsOrigins.Add(uri);
                        }
                    }
                }

                HttpContent httpcontent = new StringContent(JsonConvert.SerializeObject(c), Encoding.UTF8, "application/json");
                var put = httpclient.PutAsync(requestUri, httpcontent);
                put.Wait();
                string putJson = put.Result.Content.ReadAsStringAsync().Result;
            }
        }

        private static string RequestPasswordToken(string idsProxyUrl, string username, string password)
        {
            HttpClient httpClient = new HttpClient();
            var disco = httpClient.GetDiscoveryDocumentAsync(new DiscoveryDocumentRequest
            {
                Address = idsProxyUrl,
                Policy = { RequireHttps = false }
            });
            disco.Wait();

            var token = httpClient.RequestPasswordTokenAsync(new PasswordTokenRequest()
            {
                Address = disco.Result.TokenEndpoint,
                ClientId = "ro.identity_admin_api",
                Scope = "IdentityServerAdministrator",
                UserName = username,
                Password = password
            });

            token.Wait();

            return token.Result.AccessToken;
        }

        private static string GetIdsAuthorityFromApx(string apiServerUrl)
        {
            // in case APX is setup to use HTTPS with self-signed cert
            apiServerUrl = apiServerUrl.EndsWith("/") ? apiServerUrl : apiServerUrl + "/";
            string requestUri = apiServerUrl + "apxlogin/api/well-known/authentication-configuration";
            if (requestUri.StartsWith("https", StringComparison.InvariantCultureIgnoreCase))
            {
                InstallCertificate(requestUri);
            }

            HttpClient httpClient = new HttpClient();
            var get = httpClient.GetAsync(requestUri);
            get.Wait();
            var configuration = JsonConvert.DeserializeObject<AuthenticationConfiguration>(get.Result.Content.ReadAsStringAsync().Result);
            string idsAuthority = configuration.Issuer;

            return idsAuthority;
        }

        private static void InstallIdsCertificate(string idsBaseUrl)
        {
            if (idsBaseUrl.StartsWith("https", StringComparison.InvariantCultureIgnoreCase))
            {
                idsBaseUrl = idsBaseUrl.EndsWith("/") ? idsBaseUrl : idsBaseUrl + "/";
                string requestUri = idsBaseUrl + ".well-known/openid-configuration";
                InstallCertificate(requestUri);
            }
        }
        private static void InstallCertificate(string requestUri)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(requestUri);
            request.AllowAutoRedirect = false;
            request.ServerCertificateValidationCallback += (sender, cert, chain, sslPolicyErrors) => true;

            using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
            {
                X509Certificate2 certificate = new X509Certificate2(request.ServicePoint.Certificate);
                if (certificate != null)
                {
                    using (X509Store store = new X509Store(StoreName.Root, StoreLocation.LocalMachine))
                    {
                        store.Open(OpenFlags.OpenExistingOnly | OpenFlags.ReadWrite);
                        store.Add(certificate);
                    }
                }
            }
        }
    }
}
