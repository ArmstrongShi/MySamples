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

            var token = client.LoginNT();

            var apxRestClient = new ApxRestClient(baseAddress, token.AccessToken);
            apxRestClient.GetPortfolios();
            apxRestClient.GetBlotters();

            var apxSoapClient = new ApxSoapClient(baseAddress, token.AccessToken);
            apxSoapClient.GetContacts();

            Console.WriteLine("Press Enter to continue.");
            Console.ReadLine();
        }
    }
}
