
using System.Net.Http;

namespace Advent.ApxRest
{
    class Program
    {
        static void Main(string[] args)
        {
            string apxWebUrl = "https://vmashi.advent.com";
            AuthClient client = new AuthClient(apxWebUrl);
            //apxws = client.Login(); // Win NT user
            System.Random r = new System.Random();
            while(1==1)
            {

                int i = r.Next(1, 200);
                ApxWS apxws = client.Login($"api{i}", "advs");
                
                string json = apxws.GetBlotters();

                System.Threading.Thread.Sleep(5 * 1000);
                //client.Logout();
            }
        }
    }
}