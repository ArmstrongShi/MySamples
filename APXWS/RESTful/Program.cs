
using System.Net.Http;

namespace Advent.ApxRest
{
    class Program
    {
        static void Main(string[] args)
        {
            string apxWebUrl = "https://VMW19APXCLOUD05.GENCOS.COM";
            AuthClient client = new AuthClient(apxWebUrl);
            ApxWS apxws = client.Login("web", "advs");
            //apxws = client.Login(); // Win NT user
            string json = apxws.GetBlotters();

            client.Logout();
        }
    }
}