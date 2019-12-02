
/*
 * To use Token Based Authentication for SOAP API request, you have to add this class to your client code.
 * The namespance must be same as the namespace of web reference that you added.
 */
namespace AdvOidcSample.ApxSoap
{
    using System;
    public partial class ApxWS : System.Web.Services.Protocols.SoapHttpClientProtocol
    {
        public string AccessToken
        {
            get;
            set;
        }

        protected override System.Net.WebRequest GetWebRequest(Uri uri)
        {
            var request = base.GetWebRequest(uri);
            request.Headers.Add("Authorization", string.Format("Bearer {0}", AccessToken));
            return request;
        }
    }
}
