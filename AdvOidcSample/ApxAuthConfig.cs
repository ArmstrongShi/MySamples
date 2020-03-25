using Newtonsoft.Json;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace AdvOidcSample
{
    class ApxAuthConfig
    {
        public string Issuer
        {
            get;
            set;
        }

        public bool EnabledTokenBasedAuthentication
        {
            get;
            set;
        }

        public static ApxAuthConfig Create(string apxBaseUrl)
        {
            Uri requestUri = null;
            Uri baseUri = new Uri(apxBaseUrl);
            // You must get the issuer from this APX well-known API first, which does not require login
            string relativeUri = "apxlogin/api/well-known/authentication-configuration";
            if (Uri.TryCreate(baseUri, relativeUri, out requestUri))
            {
                var client = new HttpClient();
                var response = Task.Run(()=>client.GetAsync(requestUri)).Result;                
                var content = Task.Run(()=>response.Content.ReadAsStringAsync()).Result;
                var config = JsonConvert.DeserializeObject<ApxAuthConfig>(content);
                return config;
            }

            return null;
        }
    }
}
