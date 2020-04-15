
using System.Net.Http;

namespace Advent.ApxRest
{
    class Program
    {
        static void Main(string[] args)
        {
            string apxWebUrl = "https://apx.company.com";
            AuthClient client = new AuthClient(apxWebUrl);
            ApxWS apxws = client.Login("web", "advs");
            //apxws = client.Login(); // Win NT user
            string json = apxws.GetBlotters();

            client.Logout();
        }
    }
}