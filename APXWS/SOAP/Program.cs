namespace Advent.ApxSoap
{
    using Advent.ApxSoap.Authentication;
    using Advent.ApxSoap.Examples;

    class Program
    {
        static void Main(string[] args)
        {
            string apxWebUrl = "https://VMW19APXCLOUD05.GENCOS.COM";
            AuthClient client = new AuthClient(apxWebUrl);
            ApxWS.ApxWS apxws = client.Login("web", "advs"); 
            //apxws = client.Login(); // Win NT user

            ApxActivity.CreateNewActivity(apxws);

            client.Logout();
        }
    }
}

