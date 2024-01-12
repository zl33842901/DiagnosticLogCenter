using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace xLiAd.DiagnosticLogCenter.AgentFramework
{
    public class LogHttpModule : IHttpModule
    {
        public void Init(HttpApplication application)
        {
            application.BeginRequest += AspnetRequestRecorder.OnBeginRequest;
            application.EndRequest += AspnetRequestRecorder.OnEndRequest;
        }

        public void Dispose()
        {
            
        }
    }
}
