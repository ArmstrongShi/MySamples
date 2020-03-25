
/* To use Token Based Authentication for SOAP API request, 
 * you must add this class after adding the web reference.
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
            if (!string.IsNullOrEmpty(this.AccessToken))
            {
                request.Headers.Add("Authorization", string.Format("Bearer {0}", AccessToken));
            }

            return request;
        }
    }
}

