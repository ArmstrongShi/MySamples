using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Advent.ApxRestApiExample
{
    class ApxInternalApiProxy : ApxApiProxyBase
    {
        //api/internal/reporting/ssrs/RunReport
        public ApxInternalApiProxy(string webServer)
            : base(webServer)
        {
        }

        public ApxInternalApiProxy(string webServer, string login, string password)
            : base(webServer, login, password)
        {
        }

        public string RunSSRSReport(string request)
        {
            string url = string.Format("api/internal/reporting/ssrs/RunReport");
            return this.client.HttpPost(url, request);
        }

        public string PostUser(string request)
        {
            string url = string.Format("api/internal/user");
            return this.client.HttpPost(url, request);
        }

        public string GetUserInfo()
        {
            string url = string.Format("api/internal/UserInfo?$d=y&$m=n&$f=u");
            return this.client.HttpGet(url);
        }
    }
}
