using System.Collections.Generic;
using System.Net;
using System.Text;

namespace Discore.Http.Internal
{
    class UrlParametersBuilder : Dictionary<string, string>
    {
        public string ToQueryString()
        {
            StringBuilder sb = new StringBuilder();
            
            foreach (KeyValuePair<string, string> parameter in this)
            {
                if (parameter.Value == null)
                    break;

                if (sb.Length > 0)
                    sb.Append('&');
                else
                    sb.Append('?');

                sb.Append(WebUtility.UrlEncode(parameter.Key));
                sb.Append('=');
                sb.Append(WebUtility.UrlEncode(parameter.Value));
            }

            return sb.ToString();
        }
    }
}
