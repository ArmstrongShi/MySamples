using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.NetworkInformation;
using System.Security.AccessControl;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Security.Principal;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using IdentityModel.Client;
using Newtonsoft.Json;

// ReSharper disable InconsistentNaming
namespace Apx.Installer.Server
{
    public static class CustomAction
    {
        private const string AIDB_Identifier = "$IS_PRODDB";
        private const string AIDB_ApxRedirectUri = "$IS_APXREDIRECTURI";
        private const string AIDB_Identity_API = "$IS_ADMINAPI";
        private const string APXDB_Identifier = "$IS_PRODFIRMDB";
        private const string APX_Master_User_Identifier = "$IS_PRODLOGINNAME";
        private const string CertName = "AdventIdentity";
        private static readonly string _HostName = Dns.GetHostEntry(Dns.GetHostName()).HostName;

        // ReSharper disable once UnusedMethodReturnValue.Global
        public static int SetupIdentityServer(int handle, string dbServerName, string userName, string dbUserName, string dbPassword, string webServerName, string controlDBName, string aidDBName, string installDir, string sourceDir, string password, string isInstalled, string apxRedirectUri)
        {
            if (!Boolean.Parse(isInstalled))
            {
                return 0;
            }

            try
            {
                string logFile = Path.Combine(installDir, @"InstallLog\IdentityServerInstall.log");
                string dirPath = Path.GetDirectoryName(logFile);
                if (!String.IsNullOrEmpty(dirPath) && !Directory.Exists(dirPath))
                {
                    Directory.CreateDirectory(dirPath);
                }

                using (FileStream fs = new FileStream(logFile, FileMode.Create))
                {
                    using (StreamWriter sw = new StreamWriter(fs))
                    {
                        try
                        {
                            Environment.CurrentDirectory = sourceDir;
                            sw.WriteLine($"Current working directory: {Environment.CurrentDirectory}.");

                            sw.WriteLine("Begin getting APX Firm DB name.");
                            string firmDBName = GetFirmDBName(dbServerName, dbUserName, dbPassword, controlDBName, sw);
                            sw.WriteLine("End getting APX Firm DB name.");

                            try
                            {
                                sw.WriteLine("Begin database part.");
                                CreateDatabase(dbServerName, userName, dbUserName, dbPassword, firmDBName, aidDBName, apxRedirectUri, sw);
                                sw.WriteLine("End database part.");
                            }
                            catch (Exception e)
                            {
                                sw.WriteLine($"Error: {e.Message}.");
                                sw.WriteLine($"Error: {e.StackTrace}.");
                            }

                            if (!CertExists(out string thumbPrint))
                            {
                                sw.WriteLine("Begin cert creation.");
                                thumbPrint = CreateSelfSignedCert(userName);
                                sw.WriteLine("End cert creation.");
                            }

                            sw.WriteLine("Begin configuring netsh.");
                            ConfigureNetSh(userName, thumbPrint, sw);
                            sw.WriteLine("End configuring netsh.");

                            sw.WriteLine("Begin updating appSettings file.");
                            UpdateAppSettingsFile(installDir, dbServerName, firmDBName, aidDBName, thumbPrint);
                            sw.WriteLine("End updating appSettings file.");

                            DeleteCombaseDllIfExists(installDir);
                        }
                        catch (Exception e)
                        {
                            sw.WriteLine($"Error: {e.Message}.");
                            sw.WriteLine($"Error: {e.StackTrace}.");
                        }
                    }
                }
            }
            catch (Exception)
            {
                // Do nothing.
            }

            return 0;
        }

        private static string GetFirmDBName(string dbServerName, string dbUserName, string dbPassword, string controllerDBName, StreamWriter sw)
        {
            string apxFirmDbName = "APXFirmDb";
            SqlConnectionStringBuilder sb = GetSqlConnection(dbServerName, dbUserName, dbPassword);
            sb.InitialCatalog = controllerDBName;
            using (SqlConnection connection = new SqlConnection(sb.ToString()))
            {
                connection.Open();
                const string SqlCommand = "SELECT DatabaseName FROM CtrlFirmApplication WHERE FirmID = 1";
                using (SqlCommand command = new SqlCommand(SqlCommand, connection))
                {
                    try
                    {
                        sw.WriteLine($"...Executing {SqlCommand}");
                        apxFirmDbName = (string)command.ExecuteScalar();
                    }
                    catch (SqlException sqlEx)
                    {
                        StringBuilder errorMessages = new StringBuilder();
                        for (int x = 0; x < sqlEx.Errors.Count; ++x)
                        {
                            errorMessages.Append($"Index # {x}{Environment.NewLine}" +
                                                 $"Message: {sqlEx.Errors[x].Message}{Environment.NewLine}" +
                                                 $"Error Number: {sqlEx.Errors[x].Number}{Environment.NewLine}" +
                                                 $"LineNumber: {sqlEx.Errors[x].LineNumber}{Environment.NewLine}" +
                                                 $"Source: {sqlEx.Errors[x].Source}{Environment.NewLine}" +
                                                 $"Procedure: {sqlEx.Errors[x].Procedure}{Environment.NewLine}");
                        }

                        sw.WriteLine(errorMessages.ToString());
                    }
                    catch (Exception e)
                    {
                        sw.WriteLine(e);
                    }
                }
            }

            return apxFirmDbName;
        }

        private static void CreateDatabase(string dbServerName, string userName, string dbUserName, string dbPassword, string firmDBName, string aidDBName, string apxRedirectUri, StreamWriter sw)
        {
            sw.WriteLine("The input apxRedirectUri is " + apxRedirectUri);
            string apxRedirectHost = WebServerInstall.GetWebFQDN(apxRedirectUri).ToLower();
            sw.WriteLine("Get fqdn the apxRedirectHost is " + apxRedirectHost);

            string sqlScript = File.ReadAllText(@".\AdventIdentityDB.sql");
            sqlScript = sqlScript.Replace(AIDB_Identifier, aidDBName);
            sqlScript = sqlScript.Replace(APXDB_Identifier, firmDBName);
            sqlScript = sqlScript.Replace(APX_Master_User_Identifier, userName);
            sqlScript = sqlScript.Replace(AIDB_ApxRedirectUri, apxRedirectHost);
            sqlScript = sqlScript.Replace(AIDB_Identity_API, Environment.MachineName.ToLowerInvariant());

            SqlConnectionStringBuilder sb = GetSqlConnection(dbServerName, dbUserName, dbPassword);

            // Run the first two statements against master db.
            string[] commandStrings = Regex.Split(sqlScript, @"^\s*GO\s*$", RegexOptions.Multiline | RegexOptions.IgnoreCase).ToArray();
            using (SqlConnection connection = new SqlConnection(sb.ToString()))
            {
                connection.Open();
                for (int i = 0; i < 2; ++i)
                {
                    if (commandStrings[i].Trim() != String.Empty)
                    {
                        using (SqlCommand command = new SqlCommand(commandStrings[i], connection))
                        {
                            try
                            {
                                sw.WriteLine($"...Executing {commandStrings[i]}");
                                command.ExecuteNonQuery();
                            }
                            catch (SqlException sqlEx)
                            {
                                StringBuilder errorMessages = new StringBuilder();
                                for (int x = 0; x < sqlEx.Errors.Count; ++x)
                                {
                                    errorMessages.Append($"Index # {x}{Environment.NewLine}" +
                                                         $"Message: {sqlEx.Errors[x].Message}{Environment.NewLine}" +
                                                         $"Error Number: {sqlEx.Errors[x].Number}{Environment.NewLine}" +
                                                         $"LineNumber: {sqlEx.Errors[x].LineNumber}{Environment.NewLine}" +
                                                         $"Source: {sqlEx.Errors[x].Source}{Environment.NewLine}" +
                                                         $"Procedure: {sqlEx.Errors[x].Procedure}{Environment.NewLine}");
                                }

                                sw.WriteLine(errorMessages.ToString());
                            }
                            catch (Exception e)
                            {
                                sw.WriteLine(e);
                            }
                        }
                    }
                }
            }

            // Run the remaining statements against Advent Identity Service's db.
            sb.InitialCatalog = aidDBName;
            using (SqlConnection connection = new SqlConnection(sb.ToString()))
            {
                connection.Open();
                for (int i = 2; i < commandStrings.Length; ++i)
                {
                    if (commandStrings[i].Trim() != String.Empty)
                    {
                        using (SqlCommand command = new SqlCommand(commandStrings[i], connection))
                        {
                            try
                            {
                                sw.WriteLine($"...Executing {commandStrings[i]}");
                                command.ExecuteNonQuery();
                            }
                            catch (SqlException sqlEx)
                            {
                                StringBuilder errorMessages = new StringBuilder();
                                for (int x = 0; x < sqlEx.Errors.Count; ++x)
                                {
                                    errorMessages.Append($"Index # {x}{Environment.NewLine}" +
                                                         $"Message: {sqlEx.Errors[x].Message}{Environment.NewLine}" +
                                                         $"Error Number: {sqlEx.Errors[x].Number}{Environment.NewLine}" +
                                                         $"LineNumber: {sqlEx.Errors[x].LineNumber}{Environment.NewLine}" +
                                                         $"Source: {sqlEx.Errors[x].Source}{Environment.NewLine}" +
                                                         $"Procedure: {sqlEx.Errors[x].Procedure}{Environment.NewLine}");
                                }

                                sw.WriteLine(errorMessages.ToString());
                            }
                            catch (Exception e)
                            {
                                sw.WriteLine(e);
                            }
                        }
                    }
                }
            }
        }

        private static SqlConnectionStringBuilder GetSqlConnection(string dbServerName, string dbUserName, string dbPassword)
        {
            SqlConnectionStringBuilder sb = new SqlConnectionStringBuilder();
            {
                sb.DataSource = dbServerName;
                sb.UserID = dbUserName;
                sb.Password = dbPassword;
                sb.InitialCatalog = "master";
                sb.IntegratedSecurity = false;
                sb.MultipleActiveResultSets = true;
                sb.ConnectTimeout = 30;
            }

            return sb;
        }

        private static bool CertExists(out string thumbPrint)
        {
            using (X509Store store = new X509Store(StoreName.My, StoreLocation.LocalMachine))
            {
                store.Open(OpenFlags.ReadOnly);
                X509Certificate2Collection certificates = store.Certificates.Find(X509FindType.FindBySubjectName, _HostName, false);
                foreach (X509Certificate2 certificate in certificates)
                {
                    if (certificate.FriendlyName == CertName)
                    {
                        thumbPrint = certificate.Thumbprint;
                        store.Close();

                        // if LocalMachine/My has the cert make check to see if Root has one too if not then add it in.
                        using (X509Store store2 = new X509Store(StoreName.Root, StoreLocation.LocalMachine))
                        {
                            store2.Open(OpenFlags.ReadWrite);
                            bool isCertInRoot = false;
                            X509Certificate2Collection certificates2 = store2.Certificates.Find(X509FindType.FindBySubjectName, _HostName, false);
                            foreach (X509Certificate2 cert2 in certificates2)
                            {
                                if (cert2.FriendlyName == CertName)
                                {
                                    isCertInRoot = true;
                                    break;
                                }
                            }

                            if (!isCertInRoot)
                            {
                                store2.Add(certificate);
                                store2.Close();
                            }
                        }

                        return true;
                    }
                }

                store.Close();
            }

            thumbPrint = String.Empty;
            return false;
        }

        private static string CreateSelfSignedCert(string userName)
        {
            string thumbPrint;

            // Create a new instance of CspParameters. Pass 13 to specify a DSA container or 1 to specify an RSA container.  The default is 1.
            CspParameters cspParams = new CspParameters { ProviderType = 24, KeyContainerName = "AdventContainer", Flags = CspProviderFlags.UseMachineKeyStore, KeyNumber = (int)KeyNumber.Exchange };
            using (RSACryptoServiceProvider rsa = new RSACryptoServiceProvider(2048, cspParams))
            {
                CertificateRequest req = new CertificateRequest($"cn={_HostName}", rsa, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
                SubjectAlternativeNameBuilder sanBuilder = new SubjectAlternativeNameBuilder();
                sanBuilder.AddIpAddress(IPAddress.Loopback);
                //sanBuilder.AddIpAddress(IPAddress.IPv6Loopback);
                sanBuilder.AddDnsName("localhost");
                sanBuilder.AddDnsName(Environment.MachineName);

                // Only add Fully Qualified Domain Name if machine is part of domain.
                if (Environment.MachineName.IndexOf(_HostName, StringComparison.OrdinalIgnoreCase) < 0)
                {
                    sanBuilder.AddDnsName(_HostName);
                }

                req.CertificateExtensions.Add(sanBuilder.Build());
                using (X509Certificate2 cert = req.CreateSelfSigned(DateTimeOffset.Now, DateTimeOffset.Now.AddYears(5)))
                {
                    cert.FriendlyName = CertName;
                    thumbPrint = cert.Thumbprint;
                    using (X509Store store = new X509Store(StoreName.My, StoreLocation.LocalMachine))
                    {
                        store.Open(OpenFlags.ReadWrite);
                        store.Add(cert);
                        store.Close();
                    }

                    Thread.Sleep(2000);
                    using (X509Store store = new X509Store(StoreName.Root, StoreLocation.LocalMachine))
                    {
                        store.Open(OpenFlags.ReadWrite);
                        store.Add(cert);
                        store.Close();
                    }
                }

                string uniqueKeyContainerName = rsa.CspKeyContainerInfo.UniqueKeyContainerName;
                string keyFilePath = FindKeyLocation(uniqueKeyContainerName);
                if (!String.IsNullOrWhiteSpace(keyFilePath))
                {
                    FileInfo file = new FileInfo(keyFilePath + "\\" + uniqueKeyContainerName);
                    FileSecurity fs = file.GetAccessControl();
                    NTAccount account = new NTAccount(userName);
                    fs.AddAccessRule(new FileSystemAccessRule(account, FileSystemRights.FullControl, AccessControlType.Allow));
                    file.SetAccessControl(fs);
                }

                rsa.Clear();
            }

            return thumbPrint;
        }

        private static string FindKeyLocation(string keyFileName)
        {
            string folderPath = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData) + @"\Microsoft\Crypto\RSA\MachineKeys";
            return Directory.GetFiles(folderPath, keyFileName).Length > 0 ? folderPath : String.Empty;
        }

        private static void ConfigureNetSh(string userName, string thumbPrint, StreamWriter sw)
        {
            string netshUrlArgsAdd = $"http add urlacl url=https://{Environment.MachineName}:5001/ user={userName}";
            string netshSSLArgsAdd = $"http add sslcert ipport=0.0.0.0:5001 certhash={thumbPrint} appid={{3d543127-4632-4025-9119-b1615480532f}} certstorename=My";

            string netshUrlArgsDelete = $"http delete urlacl url=https://{Environment.MachineName}:5001/";
            const string netshSSLArgsDelete = "http delete sslcert ipport=0.0.0.0:5001";

            sw.WriteLine($"...Executing {netshUrlArgsDelete}");
            RunProcess(netshUrlArgsDelete, sw);
            sw.WriteLine($"...Executing {netshUrlArgsAdd}");
            RunProcess(netshUrlArgsAdd, sw);
            sw.WriteLine($"..Executing {netshSSLArgsDelete}");
            RunProcess(netshSSLArgsDelete, sw);
            sw.WriteLine($"...Executing {netshSSLArgsAdd}");
            RunProcess(netshSSLArgsAdd, sw);
        }

        private static void RunProcess(string args, StreamWriter sw)
        {
            StringBuilder sb = new StringBuilder();
            StringBuilder sbError = new StringBuilder();
            ProcessStartInfo psi = new ProcessStartInfo("netsh", args) { RedirectStandardOutput = true, RedirectStandardError = true, UseShellExecute = false, CreateNoWindow = true };
            using (Process p = new Process())
            {
                p.StartInfo = psi;
                p.OutputDataReceived += (sender, e) => { sb.AppendLine(e.Data); };
                p.ErrorDataReceived += (sender, e) => { sbError.AppendLine(e.Data); };
                p.Start();
                p.BeginOutputReadLine();
                p.BeginErrorReadLine();
                p.WaitForExit(5000);
            }

            if (sb.Length > 0)
            {
                sw.WriteLine(sb.ToString());
            }

            // This is greater than two just in case a blank line with just carriage-return is appended.
            if (sbError.Length > 2)
            {
                sw.WriteLine(sbError.ToString());
            }
        }

        private static void UpdateAppSettingsFile(string installDir, string dbServerName, string firmDBName, string aidDBName, string thumbprint)
        {
            string appSettingsFile = Path.Combine(installDir, @"APXCore\Bin\identityserver\appsettings.json");
            string fileContents = File.ReadAllText(appSettingsFile);
            dbServerName = dbServerName.Replace(@"\", @"\\");
            fileContents = fileContents.Replace("Server=localhost", $@"Server={dbServerName}");
            fileContents = fileContents.Replace("Database=AdventIdentityServices", $"Database={aidDBName}");
            fileContents = fileContents.Replace("Database=APXFirm", $"Database={firmDBName}");
            fileContents = fileContents.Replace("CN=AdventIdentity", thumbprint);
            fileContents = fileContents.Replace("https://localhost:5001", $"https://{Environment.MachineName}:5001");
            File.WriteAllText(appSettingsFile, fileContents);
        }

        // ReSharper disable once IdentifierTypo
        private static void DeleteCombaseDllIfExists(string installDir)
        {
            // ReSharper disable once IdentifierTypo
            // ReSharper disable once InconsistentNaming
            string combaseDLL = Path.Combine(installDir, @"APXCore\Bin\identityserver\combase.dll");
            if (File.Exists(combaseDLL))
            {
                File.Delete(combaseDLL);
            }
        }
    }
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
        [JsonProperty("clientSecrets")] public List<ClientSecret> clientSecrets { get; set; }
        [JsonProperty("properties")] public List<string> properties { get; set; }
    }

    public class Claim
    {
        [JsonProperty("Id")] public int Id { get; set; }
        [JsonProperty("Type")] public string Type { get; set; }
        [JsonProperty("Value")] public string Value { get; set; }
    }

    public class ClientSecret
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
    public class WebServerInstall
    {
        public WebServerInstall() { }
        public string SeedRedirectUris(string appServer, string webServerUrl, string idsProxyUrl, string username, string password, string installDir)
        {
            string logFile = Path.Combine(installDir, @"InstallLog\RegisterApxWebClient.log");
            string dirPath = Path.GetDirectoryName(logFile);
            idsProxyUrl = idsProxyUrl.ToLower();
            if (!String.IsNullOrEmpty(dirPath) && !Directory.Exists(dirPath))
            {
                Directory.CreateDirectory(dirPath);
            }
            using (FileStream fs = new FileStream(logFile, FileMode.Create))
            {
                using (StreamWriter sw = new StreamWriter(fs))
                {
                    string appServerFqdn = GetFQDN(appServer).ToLower();
                    string webserverFqdn = GetWebFQDN(webServerUrl).ToLower();
                    if (webserverFqdn == null)
                    {
                        sw.WriteLine("Invalid input ids URL");
                        return installDir;
                    }
                    try
                    {
                        if (!InstallIdsAppServerCertificate(appServerFqdn, sw))
                        {
                            sw.WriteLine("Failed to install cert from appserver");
                        }
                        if (GetFQDN(GetUrlHost(idsProxyUrl)) == GetFQDN(GetUrlHost(webServerUrl)))
                        {
                            if (!InstallIdsCertificate(idsProxyUrl, appServerFqdn, sw))
                            {
                                sw.WriteLine("Failed to install cert to appserver from webserver");
                            }
                        }
                        else
                        {
                            if (!InstallIdsCertificate(idsProxyUrl, sw))
                            {
                                sw.WriteLine("Failed to install cert from webserver");
                            }
                            string webOauthUrl = "https://" + GetFQDN(GetUrlHost(webServerUrl)).ToLower() + "/oauth/";
                            if (!InstallIdsCertificate(webOauthUrl, appServerFqdn, sw))
                            {
                                sw.WriteLine("Failed to install cert to appserver from " + webOauthUrl);
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        sw.WriteLine("Exception occured while installing Cert");
                        LogInstallException(sw, e);
                    }

                    try
                    {
                        bool IsSuccess = false;
                        IsSuccess = RegisterApxWebRedirectUris(appServerFqdn, idsProxyUrl, username, password, webserverFqdn, sw);
                        if (IsSuccess)
                        {
                            sw.WriteLine("Register Apx WebClient Successful.");
                            return "OK";
                        }
                        else { sw.WriteLine("Register Apx WebClient failed."); }
                    }
                    catch (Exception e)
                    {
                        sw.WriteLine("Exception occured while Registering Apx WebClient.");
                        sw.WriteLine("User name is " + username);
                        sw.WriteLine("Rediret URI is " + idsProxyUrl);
                        LogInstallException(sw, e);
                    }
                }
            }
            return installDir;
        }
        private static void LogInstallException(StreamWriter sw, Exception e)
        {
            sw.WriteLine($"Error: {e.Message}.");
            sw.WriteLine($"Error: {e.StackTrace}.");
            if (e.InnerException != null)
            {
                sw.WriteLine($"Error: {e.InnerException.Message}.");
            }
        }
        public string UpdateUrlFQDN(string webUrl)
        {
            try
            {
                string hostInputName = GetUrlHost(webUrl);
                string hostFqdnName = GetInputFQDN(hostInputName);
                return webUrl.Replace(hostInputName, hostFqdnName);
            }
            catch
            {
                return webUrl;
            }
            
        }
        public string GetoutWebFQDN(string idsProxyUrl)
        {
            return GetWebFQDN(idsProxyUrl);
        }
        public string GetoutInputFQDN(string hostInputName)
        {
            return GetInputFQDN(hostInputName);
        }
        public static string GetWebFQDN(string idsProxyUrl)
        {
            return GetInputFQDN(GetUrlHost(idsProxyUrl));
        }
        public static string GetUrlHost(string idsProxyUrl)
        {
            Uri uriAddress;
            bool isValidUri = Uri.TryCreate(idsProxyUrl, UriKind.RelativeOrAbsolute, out uriAddress);
            if (!isValidUri)
            {
                return null;
            }
            string hostInputName = uriAddress.Host;
            return hostInputName;
        }
        public static string GetInputFQDN(string hostInputName)
        {
            string hostServerName = null;
            try
            {
                hostServerName = Dns.GetHostEntry(hostInputName).HostName;
            }
            catch{}
            IPAddress address = new IPAddress(0);
            bool isIp = IPAddress.TryParse(hostInputName, out address);
            bool isMatchFQDN = hostInputName.Contains(".");
            string domainName = IPGlobalProperties.GetIPGlobalProperties().DomainName;
            if (hostInputName == hostServerName)
            {
                return hostServerName;
            }
            if (isIp)
            {
                return hostInputName;
            }
            if (isMatchFQDN)
            {
                return hostInputName;
            }
            else
            {
                if (hostServerName.EndsWith("." + domainName))
                {
                    return hostInputName + "." + domainName;
                }
                else
                {
                    return hostInputName;
                }
            }
        }

        public static string ResolveUrl(string inputUrl)
        {
            inputUrl = inputUrl.ToLower();
            Uri uri = new Uri(inputUrl);
            var resolvedHost = ResolveHost(uri.Host);
            inputUrl = inputUrl.Replace(uri.Host, resolvedHost);
            return inputUrl;
        }
        public static string ResolveHost(string host)
        {
            bool dnsResolved = false;
            try
            {
                var hostEntry = Dns.GetHostEntry(host);
                dnsResolved = true;
            }
            catch
            {
                dnsResolved = false;
            }

            if (dnsResolved)
            {
                IPAddress address = null;
                if (!IPAddress.TryParse(host, out address) && !host.Contains("."))
                {
                    var domain = IPGlobalProperties.GetIPGlobalProperties().DomainName; 
                    host = host + "." + domain;
                }
            }

            return host;
        }

        public static bool RegisterApxWebRedirectUris(string appServerFqdn, string idsProxyUrl, string idsAdminUsername, string idsAdminPassword, string webServerFqdn, StreamWriter sw)
        {
            string token = RequestPasswordToken(idsProxyUrl, idsAdminUsername, idsAdminPassword, sw);
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
            bool IsSuccess = false;
            IsSuccess = RegisterClientRedirectUris(idsProxyUrl, token, client, sw);
            return IsSuccess;
        }
        private static bool RegisterClientRedirectUris(string idsAuthority, string accessToken, Client client, StreamWriter sw)
        {
            bool IsSuccess = false;
            HttpClient httpclient = new HttpClient();
            httpclient.DefaultRequestHeaders.Add("Authorization", string.Format("Bearer {0}", accessToken));
            httpclient.DefaultRequestHeaders.Add("accept", "application/json");

            // get all clients
            idsAuthority = idsAuthority.EndsWith("/") ? idsAuthority : idsAuthority + "/";
            string requestUri = idsAuthority + "api/Clients";
            sw.WriteLine("Start to get clients: {0}", requestUri);
            var get = httpclient.GetAsync(requestUri);
            get.Wait();

            string result = get.Result.Content.ReadAsStringAsync().Result;
            if (!get.Result.IsSuccessStatusCode)
            {
                sw.WriteLine("Failed to get clients from " + requestUri);
                sw.WriteLine(get.Result.ReasonPhrase);
                sw.WriteLine(get.Result.RequestMessage);
                sw.WriteLine("Response body: {0}", result);
                return false;
            }
            sw.WriteLine("Finish to get clients");

            var clients = JsonConvert.DeserializeObject<Clients>(get.Result.Content.ReadAsStringAsync().Result);
            Client c = clients.ClientList.First<Client>(x => x.ClientId == client.ClientId);
            if (clients.ClientList.First<Client>(x => x.ClientId == client.ClientId) != null)
            {
                if (c.redirectUris == null)
                {
                    c.redirectUris = new List<string>(client.redirectUris);
                }
                else
                {
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
                    foreach (string uri in client.postLogoutRedirectUris)
                    {
                        if (!c.postLogoutRedirectUris.Any<string>(x => x == uri))
                        {
                            c.postLogoutRedirectUris.Add(uri);
                        }
                    }
                }

                HttpContent httpcontent = new StringContent(JsonConvert.SerializeObject(c), Encoding.UTF8, "application/json");
                sw.WriteLine("Start to put client: {0}", client.ClientId);
                var put = httpclient.PutAsync(requestUri, httpcontent);
                put.Wait();
                if (!put.Result.IsSuccessStatusCode)
                {
                    sw.WriteLine("Failed to put http content to " + requestUri);
                    sw.WriteLine(put.Result.ReasonPhrase);
                    sw.WriteLine(put.Result.RequestMessage);
                    sw.WriteLine("Response body: {0}", put.Result.Content.ReadAsStringAsync().Result);
                    return false;
                }
                IsSuccess = put.Result.IsSuccessStatusCode;
                sw.WriteLine("Finish to put client");
            } 
            return IsSuccess;

        }
        private static string RequestPasswordToken(string idsProxyUrl, string username, string password, StreamWriter sw)
        {
            HttpClient httpClient = new HttpClient();
            sw.WriteLine("start to get discovery document: {0}", idsProxyUrl);
            var disco = httpClient.GetDiscoveryDocumentAsync(new DiscoveryDocumentRequest
            {
                Address = idsProxyUrl,
                Policy = { RequireHttps = false }
            });
            disco.Wait();
            if (disco.Result.IsError)
            {
                sw.WriteLine("The discovery document request failed.");
                sw.WriteLine(disco.Result.Error);
                sw.WriteLine(disco.Result.HttpErrorReason);
                sw.WriteLine("ErrorType: {0}; Error: {1}", disco.Result.ErrorType, disco.Result.Error);
                if (disco.Result.Exception != null)
                {
                    sw.WriteLine("Exception message: {0}", disco.Result.Exception.Message);
                }
                return null;
            }
            sw.WriteLine("Finish to get discovery document");
            sw.WriteLine("Start to request token: {0}", disco.Result.TokenEndpoint);
            var token = httpClient.RequestPasswordTokenAsync(new PasswordTokenRequest()
            {
                Address = disco.Result.TokenEndpoint,
                ClientId = "ro.identity_admin_api",
                Scope = "IdentityServerAdministrator",
                UserName = username,
                Password = password
            });

            token.Wait();
            var tokenResponse = token.Result;
            if (tokenResponse == null)
            {
                sw.WriteLine("The token response is null");
            }

            if (tokenResponse.IsError)
            {
                sw.WriteLine("The token response is Error");
                sw.WriteLine(tokenResponse.Error);
                sw.WriteLine(tokenResponse.HttpErrorReason);
                sw.WriteLine("ErrorType: {0}; Error: {1}", tokenResponse.ErrorType, tokenResponse.Error);
                if (disco.Result.Exception != null)
                {
                    sw.WriteLine("Exception message: {0}", tokenResponse.Exception.Message);
                }
            }
            sw.WriteLine("Finish to request token");
            return token.Result.AccessToken;
        }

        public static bool InstallIdsAppServerCertificate(string appServerFqdn, StreamWriter sw)
        {
            string requestUri = "https://" + appServerFqdn + ":5001/";
            return InstallIdsCertificate(requestUri, sw);
        }
        private static bool InstallIdsCertificate(string idsBaseUrl, StreamWriter sw)
        {
            if (idsBaseUrl.StartsWith("https", StringComparison.InvariantCultureIgnoreCase))
            {
                idsBaseUrl = idsBaseUrl.EndsWith("/") ? idsBaseUrl : idsBaseUrl + "/";
                string requestUri = idsBaseUrl + ".well-known/openid-configuration";
                return InstallCertificate(requestUri, sw);
            }
            return true;
        }
        private static bool InstallIdsCertificate(string idsBaseUrl, string remoteAppServer, StreamWriter sw)
        {
            if (idsBaseUrl.StartsWith("https", StringComparison.InvariantCultureIgnoreCase))
            {
                idsBaseUrl = idsBaseUrl.EndsWith("/") ? idsBaseUrl : idsBaseUrl + "/";
                string requestUri = idsBaseUrl + ".well-known/openid-configuration";
                return InstallCertificate(requestUri, sw, remoteAppServer);
            }
            return true;
        }
        private static bool InstallCertificate(string requestUri, StreamWriter sw, string computer = null)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(requestUri);
            request.AllowAutoRedirect = false;
            request.ServerCertificateValidationCallback += (sender, cert, chain, sslPolicyErrors) => true;

            sw.WriteLine("Start install certificate from URL: {0}", requestUri);
            using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
            {
                if (response.StatusCode != HttpStatusCode.OK)
                {
                    sw.WriteLine("Failed to get cert from URL " + requestUri);
                    sw.WriteLine(response.StatusDescription);
                    sw.WriteLine("Status code " + response.StatusCode);
                    return false;
                }
                X509Certificate2 certificate = new X509Certificate2(request.ServicePoint.Certificate);
                if (certificate != null)
                {
                    string storeName = string.Format(@"\\{0}\root", computer == null ? GetFQDN() : computer);
                    using (X509Store store = new X509Store(storeName, StoreLocation.LocalMachine))
                    {
                        store.Open(OpenFlags.OpenExistingOnly | OpenFlags.ReadWrite);
                        store.Add(certificate);
                        if (!store.Certificates.Contains(certificate))
                        {
                            sw.WriteLine("Failed to find the added cert");
                            return false;
                        }
                    }
                }
            }
            return true;
        }
        public static string GetFQDN(string hostname)
        {
            return Dns.GetHostEntry(hostname).HostName;
        }

        public static string GetFQDN()
        {
            return GetFQDN(Dns.GetHostName());
        }
    }
}
