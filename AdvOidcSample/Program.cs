namespace AdvOidcSample
{
    using System;

    class Program
    {
        static void Main(string[] args)
        {
            string baseAddress = "http://vmapxba9.advent.com";
            var client = new ApxOidcClient
            {
                BaseAddress = baseAddress,
                ClientId = "ro.APXAPIClient",
                ClientSecret = "advs",
                Scope = "apxapi offline_access"
            };

            var token = client.PasswordLogin("api","advs");

            var apxRestClient = new ApxRestClient(baseAddress, token.AccessToken);
            apxRestClient.HttpGet("apxlogin/api/odata/v1/portfolios");
            apxRestClient.HttpGet("apxlogin/api/v2/blotters");

            var apxSoapClient = new ApxSoapClient(baseAddress, token.AccessToken);
            apxSoapClient.GetContacts();

            Console.WriteLine("Press Enter to continue.");
            Console.ReadLine();
        }
    }
}
