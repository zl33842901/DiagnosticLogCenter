using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace xLiAd.DiagnosticLogCenter.AgentFramework
{
    public class HttpWebResponseX
    {
        private Stream stream;
        private readonly HttpWebResponse response;
        public HttpWebResponseX(HttpWebResponse response)
        {
            this.response = response;
        }
        public void SetStream(Stream stream)
        {
            this.stream = stream;
        }
        public Stream GetResponseStream()
        {
            return stream;
        }
        public void Close()
        {
            stream.Close();
        }

        public string StatusDescription
        {
            get
            {
                return response.StatusDescription;
            }
        }

        public WebHeaderCollection Headers
        {
            get
            {
                return response.Headers;
            }
        }

        public string CharacterSet => response.CharacterSet;

        public string ContentEncoding => response.ContentEncoding;

        public HttpStatusCode StatusCode => response.StatusCode;
        public Uri ResponseUri => response.ResponseUri;
        public CookieCollection Cookies
        {
            get
            {
                return response.Cookies;
            }
            set
            {
                response.Cookies = value;
            }
        }
    }
}
