
namespace Advent.ApxWebServerSetup
{
    using IdentityServerHelper;
    class Program
    {
        static void Main(string[] args)
        {
            // there's an issue that blocks using application server FQDN.
            // If you want to use FQDN, you can edit this setting
            // %install_dir%\Advent\APX\APXCore\Bin\identityserver\appsettings.json
            // Find configuation
            // "AdminApiConfiguration"\"IdentityServerBaseUrl"
            // set server name to FQDN
            string applicationServerFqdn = "vmapxba9.advent.com";
            string webserverFqdn = "vmapxba8.advent.com";
            string idsAdminUsername = "admin";
            string idsAdminPassword = "advs";

            // only do this when web server and application server are installed on different machines.
            // for example, when Web Server is checked and Application Server is not checked.
            IdentityServerHelper helper = new IdentityServerHelper(applicationServerFqdn);
            //helper.InstallCertificate();
            helper.RegisterApxWebClient(webserverFqdn, idsAdminUsername, idsAdminPassword);
        }
    }
}
