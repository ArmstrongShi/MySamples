namespace AdvOidcSample
{
    using System;
    using System.Net.Http;

    class ApxRestClient
    {
        private HttpClient client;

        public ApxRestClient(string baseAddress, string accessToken)
        {
            this.client = new HttpClient();
            this.client.DefaultRequestHeaders.Add("Authorization", string.Format("Bearer {0}", accessToken));
            this.client.BaseAddress = new Uri(baseAddress);
        }

        public string HttpGet(string relativeUri)
        {
            Console.WriteLine("===========Start REST Request : {0}===========", relativeUri);
            Uri requestUri = null;
            string result = null;
            if (Uri.TryCreate(this.client.BaseAddress, relativeUri, out requestUri))
            {
                HttpResponseMessage response = this.client.GetAsync(requestUri).Result;
                result = response.Content.ReadAsStringAsync().Result;
            }

            Console.WriteLine(result);
            Console.WriteLine("===========End REST Request : {0}===========", relativeUri);
            return result;
        }
    }
}
