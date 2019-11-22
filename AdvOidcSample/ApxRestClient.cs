namespace AdvOidcSample
{
    using System;
    using System.Net.Http;

    class ApxRestClient
    {
        private HttpClient client = new HttpClient();

        public ApxRestClient (string baseAddress,string accessToken)
        {
            this.client.DefaultRequestHeaders.Add("Authorization", string.Format("Bearer {0}", accessToken));
            this.client.BaseAddress = new Uri(baseAddress);
        }

        public void GetBlotters()
        {
            string blotters = this.HttpGet("apxlogin/api/v2/blotters?$select=BlotterName");
            Console.WriteLine("===========Start Printing Blotters===========");
            Console.WriteLine(blotters);
            Console.WriteLine("===========End Printing Blotters===========");
        }

        public void GetPortfolios()
        {
            string portfolios = this.HttpGet("apxlogin/api/odata/v1/Portfolios?$select=PortfolioCode");
            Console.WriteLine("===========Start Printing Portfolios===========");
            Console.WriteLine(portfolios);
            Console.WriteLine("===========End Printing Portfolios===========");
        }

        private string HttpGet(string relativeUri)
        {
            Uri requestUri = null;
            if (Uri.TryCreate(this.client.BaseAddress, relativeUri, out requestUri))
            {
                HttpResponseMessage response = this.client.GetAsync(requestUri).Result;
                return response.Content.ReadAsStringAsync().Result;
            }

            return null;
        }
    }
}
