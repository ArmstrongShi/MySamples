namespace AdvOidcSample
{
    class Program
    {
        static void Main(string[] args)
        {
            string apxBaseUrl = "https://VMW19APXCLOUD05.gencos.com";
            ApxAuthConfig authConfig = ApxAuthConfig.Create(apxBaseUrl);
            AdvOidcClient oidcClient = new AdvOidcClient(authConfig.Issuer);
            var token = oidcClient.Login("web", "advs");
            //var token = oidcClient.Login();

            ApxRestClient restClient = new ApxRestClient(apxBaseUrl, token.AccessToken);
            restClient.HttpGet("apxlogin/api/v2/blotters");
            restClient.HttpGet("apxlogin/api/odata/v1/portfolios");

            ApxSoapClient soapClient = new ApxSoapClient(apxBaseUrl, token.AccessToken);
            soapClient.GetContacts();          
        }
    }
}
