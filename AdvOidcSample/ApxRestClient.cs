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

        public void GetBlottersV2()
        {
            string blotters = this.HttpGet("apxlogin/api/v2/blotters?$select=BlotterName");
            Console.WriteLine("===========Start Printing Blotters V2===========");
            Console.WriteLine(blotters);
            Console.WriteLine("===========End Printing Blotters V2===========");
        }

        public void GetBlottersV1()
        {
            string blotters = this.HttpGet("apxlogin/api/v1/blotters");
            Console.WriteLine("===========Start Printing Blotters V1===========");
            Console.WriteLine(blotters);
            Console.WriteLine("===========End Printing Blotters V1===========");
        }

        public void GetPortfoliosOdata()
        {
            string portfolios = this.HttpGet("apxlogin/api/odata/v1/Portfolios?$select=PortfolioCode");
            Console.WriteLine("===========Start Printing Portfolios Public API===========");
            Console.WriteLine(portfolios);
            Console.WriteLine("===========End Printing Portfolios Public API===========");
        }

        public void GetPortfoliosInternal()
        {
            string portfolios = this.HttpGet("apxlogin/api/internal/Portfolio?$m=n&$f=s");
            Console.WriteLine("===========Start Printing Portfolios Internal API===========");
            Console.WriteLine(portfolios);
            Console.WriteLine("===========End Printing Portfolios Internal API===========");
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
