namespace Advent.ApxSoap
{
    using Advent.ApxSoap.Authentication;
    using Advent.ApxSoap.Examples;

    class Program
    {
        static void Main(string[] args)
        {
            AuthClient client = new AuthClient("http://vmapxba9.advent.com");
            ApxWS.ApxWS apxws = client.Login("api", "advs"); // DB User
            //apxws = client.Login(); // Win NT user

            ApxActivity.Sample_CreateNewActivity(apxws);
        }
    }
}

