namespace AdvOidcSample
{
    using System;
    using System.Net.Http;

    class ApxRestClient
    {
        private HttpClient client = new HttpClient();

        public ApxRestClient(string baseAddress, string accessToken)
        {
            this.client.DefaultRequestHeaders.Add("Authorization", string.Format("Bearer {0}", accessToken));
            this.client.BaseAddress = new Uri(baseAddress);
        }

        public string HttpGet(string relativeUri)
        {
            Uri requestUri = null;
            if (Uri.TryCreate(this.client.BaseAddress, relativeUri, out requestUri))
            {
                Console.WriteLine("===========Start : {0}===========", relativeUri);
                HttpResponseMessage response = this.client.GetAsync(requestUri).Result;
                string result = response.Content.ReadAsStringAsync().Result;
                Console.WriteLine(result);
                Console.WriteLine("===========End : {0}===========", relativeUri);
                return result;
            }

            return null;
        }
    }
}
