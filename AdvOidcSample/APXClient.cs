namespace AdvOidcSample
{
    using System;
    using System.Net;
    using System.Net.Http;

    class APXClient
    {
        private HttpClient httpClient;

        public APXClient (string serviceUrl,string accessToken)
        {
            HttpClientHandler clientHandler = new HttpClientHandler();
            clientHandler.UseDefaultCredentials = true;

            this.httpClient = new HttpClient(clientHandler);
            httpClient.DefaultRequestHeaders.Add("Accept", "application/json");
            httpClient.DefaultRequestHeaders.Add("Authorization", string.Format("Bearer {0}", accessToken));
            httpClient.BaseAddress = new Uri(string.Format("{0}/apxis/api/odata/v1", serviceUrl));
        }

        public string GetMetadata()
        {
            return this.HttpGet("$metadata");
        }

        public string GetPortfolios()
        {
            return this.HttpGet("Portfolios");
        }

        private string HttpGet(string relativeUri)
        {
            string requestUrl = string.Format("{0}/{1}", this.httpClient.BaseAddress.AbsoluteUri, relativeUri);
            HttpResponseMessage response = this.httpClient.GetAsync(requestUrl).Result;
            return response.Content.ReadAsStringAsync().Result;
        }
    }
}
