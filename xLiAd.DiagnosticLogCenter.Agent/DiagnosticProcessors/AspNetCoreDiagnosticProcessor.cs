using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc.Abstractions;
using System;
using System.Collections.Generic;
using System.Text;

namespace xLiAd.DiagnosticLogCenter.Agent.DiagnosticProcessors
{
    public class AspNetCoreDiagnosticProcessor : ITracingDiagnosticProcessor
    {
        public string ListenerName { get; } = "Microsoft.AspNetCore";
        [DiagnosticName("Microsoft.AspNetCore.Hosting.BeginRequest")]
        public void BeginRequest([Property] HttpContext httpContext)
        {
            var path = httpContext.Request.Path;
            var ip = httpContext.Connection.RemoteIpAddress.ToString();
            var url = httpContext.Request.GetDisplayUrl();
            var method = httpContext.Request.Method;
            
        }

        [DiagnosticName("Microsoft.AspNetCore.Hosting.EndRequest")]
        public void EndRequest([Property] HttpContext httpContext)
        {
            
        }

        [DiagnosticName("Microsoft.AspNetCore.Diagnostics.UnhandledException")]
        public void DiagnosticUnhandledException([Property] HttpContext httpContext, [Property] Exception exception)
        {
            
        }

        [DiagnosticName("Microsoft.AspNetCore.Hosting.UnhandledException")]
        public void HostingUnhandledException([Property] HttpContext httpContext, [Property] Exception exception)
        {
            
        }

        //[DiagnosticName("Microsoft.AspNetCore.Mvc.BeforeAction")]
        public void BeforeAction([Property] ActionDescriptor actionDescriptor, [Property] HttpContext httpContext)
        {
        }

        //[DiagnosticName("Microsoft.AspNetCore.Mvc.AfterAction")]
        public void AfterAction([Property] ActionDescriptor actionDescriptor, [Property] HttpContext httpContext)
        {
        }
    }
}
