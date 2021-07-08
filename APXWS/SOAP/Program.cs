namespace Advent.ApxSoap
{
    using Advent.ApxSoap.Authentication;
    using Advent.ApxSoap.Examples;

    class Program
    {
        static void Main(string[] args)
        {
            string apxWebUrl = "https://vmashi.advent.com";
            AuthClient client = new AuthClient(apxWebUrl);
            ApxWS.ApxWS apxws = client.Login("api", "advs");
            //apxws = client.Login(); // Win NT user

            ApxUser.ResetApiUserPassword(apxws);

            client.Logout();
        }
    }
}

