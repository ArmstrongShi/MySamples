using IdentityModel.Client;
using IdentityModel.OidcClient;
using System;

namespace AdvOidcSample
{
    class Program
    {
        static void Main(string[] args)
        {
            var result = Program.PasswordFlow();
            Print(result);
      
           
        }

        static LoginResult AuthorizationCodeFlow()
        {
            Console.WriteLine("Authorization Code flow");
            var client = new AdvIdentityClient
            {
                Issuer = "http://vmapxba9.advent.com",
                ClientId = "authcode.apxui",
                ClientSecret = "advs",
                Scope = "openid apxapi offline_access"
            };

            var result = client.Signin();
            return result;
        }

        static TokenResponse PasswordFlow()
        {
            Console.WriteLine("Resource Owner Passord flow");
            var client = new AdvIdentityClient
            {
                Issuer = "http://vmapxba9.advent.com",
                ClientId = "ro.APXAPIClient",
                ClientSecret = "advs",
                Scope = "apxapi offline_access"
            };

            var result = client.Login("admin", "advs");
            return result;
        }

        static TokenResponse WindowsAuthFlow()
        {
            Console.WriteLine("Windows Authentiation flow");
            var client = new AdvIdentityClient
            {
                Issuer = "http://vmapxba9.advent.com",
                ClientId = "ro.APXAPIClient",
                ClientSecret = "advs",
                Scope = "apxapi offline_access"
            };

            var result = client.Login();
            return result;
        }

        static void Print(TokenResponse reponse)
        {
            Console.WriteLine("========================================================");
            if (reponse == null)
            {
                Console.WriteLine("Object \"reponse\" is null.");
            }
            else if (reponse.IsError)
            {
                Console.WriteLine(reponse.ErrorDescription);
            }
            else
            {
                Console.WriteLine("IdentityToken");
                Console.WriteLine(reponse.IdentityToken);
                Console.WriteLine("========================================================");
                Console.WriteLine("AccessToken");
                Console.WriteLine(reponse.AccessToken);
                Console.WriteLine("========================================================");
                Console.WriteLine("RefreshToken");
                Console.WriteLine(reponse.RefreshToken);
            }

            Console.WriteLine("========================================================");
            Console.WriteLine("Press Enter to continue.");
            Console.ReadLine();
        }

        static void Print(LoginResult result)
        {
            Console.WriteLine("================================================");
            if (result == null)
            {
                Console.WriteLine("Object \"result\" is null.");
            }
            else if (result.IsError)
            {
                Console.WriteLine(result.Error);
            }
            else
            {
                Console.WriteLine("Id_Token:", result.IdentityToken);
                Console.WriteLine(result.IdentityToken);
                Console.WriteLine("================================================");
                Console.WriteLine("Refresh_Token:", result.RefreshToken);
                Console.WriteLine(result.RefreshToken);
                Console.WriteLine("================================================");
                Console.WriteLine("Access_Token:", result.AccessToken);
                Console.WriteLine(result.AccessToken);
            }

            Console.WriteLine("================================================");
            Console.WriteLine("Press Enter to continue.");
            Console.ReadLine();
        }
    }
}
