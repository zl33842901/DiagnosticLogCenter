using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace xLiAd.DiagnosticLogCenter.AgentFramework
{
    public class HttpWebRequestX
    {
        private readonly string requesturl;
        private readonly Uri requesturi;
        private readonly HttpWebRequest httpWebRequest;
        public HttpWebRequestX(string url) : base()
        {
            this.requesturl = url;
            this.requesturi = new Uri(url);
            httpWebRequest = (HttpWebRequest)WebRequest.Create(this.requesturi);
        }
        public HttpWebRequestX(Uri uri) : base()
        {
            this.requesturl = uri.OriginalString;
            this.requesturi = uri;
            httpWebRequest = (HttpWebRequest)WebRequest.Create(uri);
        }

        public string Method
        {
            get
            {
                return httpWebRequest.Method;
            }
            set
            {
                httpWebRequest.Method = value;
            }
        }

        public string ContentType
        {
            get
            {
                return httpWebRequest.ContentType;
            }
            set
            {
                httpWebRequest.ContentType = value;
            }
        }

        public long ContentLength
        {
            get
            {
                return httpWebRequest.ContentLength;
            }
            set
            {
                httpWebRequest.ContentLength = value;
            }
        }
        public WebHeaderCollection Headers
        {
            get
            {
                return httpWebRequest.Headers;
            }
            set
            {
                httpWebRequest.Headers = value;
            }
        }
        public int Timeout
        {
            get
            {
                return httpWebRequest.Timeout;
            }
            set
            {
                httpWebRequest.Timeout = value;
            }
        }
        public CookieContainer CookieContainer
        {
            get
            {
                return httpWebRequest.CookieContainer;
            }
            set
            {
                httpWebRequest.CookieContainer = value;
            }
        }

        public System.Net.ServicePoint ServicePoint
        {
            get
            {
                return httpWebRequest.ServicePoint;
            }
        }

        public Version ProtocolVersion
        {
            get => httpWebRequest.ProtocolVersion;
            set => httpWebRequest.ProtocolVersion = value;
        }
        public bool KeepAlive
        {
            get => httpWebRequest.KeepAlive;
            set => httpWebRequest.KeepAlive = value;
        }
        public int ReadWriteTimeout
        {
            get => httpWebRequest.ReadWriteTimeout;
            set => httpWebRequest.ReadWriteTimeout = value;
        }
        public string Accept
        {
            get => httpWebRequest.Accept;
            set => httpWebRequest.Accept = value;
        }
        public string UserAgent
        {
            get => httpWebRequest.UserAgent;
            set => httpWebRequest.UserAgent = value;
        }
        public ICredentials Credentials
        {
            get => httpWebRequest.Credentials;
            set => httpWebRequest.Credentials = value;
        }
        public string Referer
        {
            get => httpWebRequest.Referer;
            set => httpWebRequest.Referer = value;
        }
        public bool AllowAutoRedirect
        {
            get => httpWebRequest.AllowAutoRedirect;
            set => httpWebRequest.AllowAutoRedirect = value;
        }
        public int MaximumAutomaticRedirections
        {
            get => httpWebRequest.MaximumAutomaticRedirections;
            set => httpWebRequest.MaximumAutomaticRedirections = value;
        }
        public System.Security.Cryptography.X509Certificates.X509CertificateCollection ClientCertificates
        {
            get => httpWebRequest.ClientCertificates;
            set => httpWebRequest.ClientCertificates = value;
        }
        public IWebProxy Proxy
        {
            get => httpWebRequest.Proxy;
            set => httpWebRequest.Proxy = value;
        }
        public DateTime IfModifiedSince
        {
            get => httpWebRequest.IfModifiedSince;
            set => httpWebRequest.IfModifiedSince = value;
        }
        public string Host
        {
            get => httpWebRequest.Host;
            set => httpWebRequest.Host = value;
        }
        public DateTime Date
        {
            get => httpWebRequest.Date;
            set => httpWebRequest.Date = value;
        }


        public Stream GetRequestStream()
        {
            return httpWebRequest.GetRequestStream();
        }

        public HttpWebResponseX GetResponse()
        {
            HttpWebResponseX result;

            var operationName = this.requesturl;
            var networkAddress = $"{this.requesturi.Host}:{this.requesturi.Port}";
            try
            {
                var response = httpWebRequest.GetResponse();

                Stream myResponseStream = response.GetResponseStream();

                MemoryStream ms = new MemoryStream();
                myResponseStream.CopyTo(ms);
                ms.Seek(0, SeekOrigin.Begin);
                StreamReader myStreamReader = new StreamReader(ms, Encoding.GetEncoding("utf-8"));
                string retString = myStreamReader.ReadToEnd();
                //myStreamReader.Close();
                ms.Seek(0, SeekOrigin.Begin);

                result = new HttpWebResponseX((HttpWebResponse)response);
                result.SetStream(ms);

                return result;
            }
            catch (Exception exception)
            {
                throw;
            }
            finally
            {

            }

            return null;
        }
    }
}
