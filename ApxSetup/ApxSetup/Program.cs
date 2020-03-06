
namespace Advent.ApxSetup
{
    class Program
    {
        static void Main(string[] args)
        {
            // When Web Server and App Server are different machines, we must do following
            string protocol = "https";
            string appServerFqdn = "vmapxba9.advent.com";
            string webServerFqdn = "vmapxba9.advent.com";
            string newUIServerFqdn = "vmapxba9.advent.com";
            Test(protocol, appServerFqdn, webServerFqdn, newUIServerFqdn);
        }

        static void Test(string protocol, string appServerFqdn, string webServerFqdn, string newUIServerFqdn)
        {
            // 1. Install Identity Server Self-signed Cert on Web Machine
            IdentityServerHelper.InstallIdsAppServerCertificate(appServerFqdn);

            // 2. Register APX Web's redirect urls to Identity Server
            string idsProxyUrl = protocol + "://" + webServerFqdn + "/oauth/";
            IdentityServerHelper.RegisterApxWebRedirectUris(idsProxyUrl, "admin", "advs", webServerFqdn);

            // 3. Register APX New UI's redirect Urls to Identity Server
            string apiServerUrl = protocol + "://" + webServerFqdn + "/";
            IdentityServerHelper.RegisterNewUIRedirectUris(apiServerUrl, "admin", "advs", newUIServerFqdn);
        }
    }
}
