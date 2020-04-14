using System.Net.Http;
using System.Text;

namespace Advent.ApxRest
{
    public class ApxWS
    {
        HttpClient client;
        /// <summary>
        /// Connect Apx REST Web Service through Windows NT user
        /// </summary>
        /// <param name="webServer">Web Server Name</param>
        public ApxWS(HttpClient client)
        {
            this.client = client;
        }

        public string GetBlotters(string queryOptions = null)
        {
            string url = $"apxlogin/api/v2/blotters";
            url = string.IsNullOrWhiteSpace(queryOptions) ? url : $"{url}?{queryOptions}";
            return this.HttpGet(url);
        }

        public string CreateBlotter(string request)
        {
            string url = "apxlogin/api/v2/blotters";
            return this.HttpPost(url, request);
        }

        public string UpdateBlotter(string guid, string request)
        {
            string url = $"apxlogin/api/v2/blotters({guid})";
            return this.HttpPatch(url, request);
        }

        public string DeleteBlotter(string guid)
        {
            string url = $"apxlogin/api/v2/blotters({guid})/delete";
            return this.HttpPost(url);
        }

        public string GetBloterLines(string guid, string queryOptions)
        {
            string url = $"apxlogin/api/v2/blotters({guid})/GetBlotterLines";
            url = string.IsNullOrWhiteSpace(queryOptions) ? url : $"{url}?{queryOptions}";
            return this.HttpGet(url);
        }

        public string AppendBlotterLines(string guid, string request)
        {
            string url = $"apxlogin/api/v2/blotters({guid})/AppendBlotterLines";
            return this.HttpPost(url, request);
        }

        public string DeleteBlotterLines(string guid, string request)
        {
            string url = $"apxlogin/api/v2/blotters({guid})/DeleteBlotterLines";
            return this.HttpPost(url, request);
        }

        public string DeleteBlotterLines(string guid)
        {
            string url = $"apxlogin/api/v2/blotters({guid})/DeleteBlotterLines";
            return this.HttpPost(url);
        }

        public string PostBlotter(string guid)
        {
            string url = $"apxlogin/api/v2/blotters({guid})/PostBlotter";
            return this.HttpPost(url);
        }

        public string PostBlotter(string guid, string request)
        {
            string url = $"apxlogin/api/v2/blotters({guid})/PostBlotter";
            return this.HttpPost(url, request);
        }

        /// <summary>
        /// Http Get method
        /// </summary>
        /// <param name="relativeUri">Relative Uri</param>
        /// <returns>Result of Http Get method</returns>
        private string HttpGet(string relativeUri)
        {
            string requestUrl = string.Format("{0}/{1}", this.client.BaseAddress.AbsoluteUri, relativeUri);
            HttpResponseMessage response = this.client.GetAsync(requestUrl).Result;
            return response.Content.ReadAsStringAsync().Result;
        }

        private string HttpPatch(string relativeUri, string jsonString)
        {
            string requestUrl = string.Format("{0}/{1}", this.client.BaseAddress.AbsoluteUri, relativeUri);
            HttpRequestMessage request = new HttpRequestMessage(new HttpMethod("Patch"), requestUrl);
            request.Content = new StringContent(jsonString, Encoding.UTF8, "application/json");
            HttpResponseMessage response = this.client.SendAsync(request).Result;
            return response.Content.ReadAsStringAsync().Result;
        }

        /// <summary>
        /// Http Post method
        /// </summary>
        /// <param name="relativeUri">Relative Uri</param>
        /// <param name="jsonString">json string to post.</param>
        /// <returns>Result of Http Post method</returns>
        private string HttpPost(string relativeUri, string jsonString)
        {
            string requestUrl = string.Format("{0}/{1}", this.client.BaseAddress.AbsoluteUri, relativeUri);
            HttpContent content = new StringContent(jsonString, Encoding.UTF8, "application/json");
            HttpResponseMessage response = this.client.PostAsync(requestUrl, content).Result;
            return response.Content.ReadAsStringAsync().Result;
        }

        /// <summary>
        /// Send Post request without input arguments
        /// </summary>
        /// <param name="relativeUri">Relative Uri</param>
        /// <returns>result</returns>
        private string HttpPost(string relativeUri)
        {
            string requestUrl = string.Format("{0}/{1}", this.client.BaseAddress.AbsoluteUri, relativeUri);
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, requestUrl);
            HttpResponseMessage response = this.client.SendAsync(request).Result;
            return response.Content.ReadAsStringAsync().Result;
        }

        /// <summary>
        /// Http Delete method
        /// </summary>
        /// <param name="relativeUri">Relative Uri</param>
        /// <returns>Result of Http Delete method.</returns>
        private string HttpDelete(string relativeUri)
        {
            string requestUrl = string.Format("{0}/{1}", this.client.BaseAddress.AbsoluteUri, relativeUri);
            HttpResponseMessage response = this.client.DeleteAsync(requestUrl).Result;
            return response.Content.ReadAsStringAsync().Result;
        }
    }
}
