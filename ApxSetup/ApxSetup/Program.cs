
namespace Advent.ApxSetup
{
    class Program
    {
        static void Main(string[] args)
        {
            // When Web Server and App Server are different machines, we must do following
            Test_HTTP();
            //Test_HTTPS();
        }

        static void Test_HTTP()
        {
            string appServerFqdn = "vmapxba9.advent.com";
            string webServerFqdn = "vmapxba9.advent.com";
            string newUIServerFqdn = "vmapxba9.advent.com";
            // 1. Install Identity Server Self-signed Cert on Web Machine
            //IdentityServerHelper.InstallIdsAppServerCertificate(appServerFqdn);

            // 2. Register APX Web's redirect urls to Identity Server
            IdentityServerHelper.RegisterApxWebRedirectUris("http://" + webServerFqdn + "/oauth/", "admin", "advs", webServerFqdn);

            // 3. Register APX New UI's redirect Urls to Identity Server
            //IdentityServerHelper.RegisterNewUIRedirectUris("http://" + webServerFqdn + "/", "admin", "advs", newUIServerFqdn);
        }

        static void Test_HTTPS()
        {
            string appServerFqdn = "vmapxba8.advent.com";
            string webServerFqdn = "vmapxba8.advent.com";
            string newUIServerFqdn = "vmapxba8.advent.com";
            // 1. Install Identity Server Self-signed Cert on Web Machine
            IdentityServerHelper.InstallIdsAppServerCertificate(appServerFqdn);

            // 2. Register APX Web's redirect urls to Identity Server
            IdentityServerHelper.RegisterApxWebRedirectUris("https://" + webServerFqdn + "/oauth/", "admin", "advs", webServerFqdn);

            // 3. Register APX New UI's redirect Urls to Identity Server
            IdentityServerHelper.RegisterNewUIRedirectUris("https://" + webServerFqdn + "/", "admin", "advs", newUIServerFqdn);
        }
    }
}
