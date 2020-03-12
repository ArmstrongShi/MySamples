using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Advent.ApxSoap.AuthWS
{
    public partial class AuthenticateWS : System.Web.Services.Protocols.SoapHttpClientProtocol
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
