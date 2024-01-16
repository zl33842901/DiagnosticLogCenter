using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Web.Infrastructure.DynamicModuleHelper;
using System.Web;
using xLiAd.DiagnosticLogCenter.AgentFramework;
using System.Diagnostics;

[assembly: PreApplicationStartMethod(typeof(InitHelper), nameof(InitHelper.Init))]
namespace xLiAd.DiagnosticLogCenter.AgentFramework
{
    public class InitHelper
    {
        public static void Init()
        {
            DiagnosticLogConfig config = new DiagnosticLogConfig();
            var appsettings = System.Configuration.ConfigurationManager.AppSettings;
            config.Enable = appsettings["DiagnosticLogEnable"] == "1" || appsettings["DiagnosticLogEnable"] == "true";
            //config.EnableAspNetCore = appsettings["DiagnosticLogEnableAspNetCore"] == "1" || appsettings["DiagnosticLogEnableAspNetCore"] == "true";
            //config.Enable = appsettings["DiagnosticLogEnable"] == "1" || appsettings["DiagnosticLogEnable"] == "true";
            //config.Enable = appsettings["DiagnosticLogEnable"] == "1" || appsettings["DiagnosticLogEnable"] == "true";
            config.CollectServerAddress = appsettings["DiagnosticLogCollectServerAddress"];
            config.ClientName = appsettings["DiagnosticLogClientName"];
            config.EnvName = appsettings["DiagnosticLogEnvName"];
            config.AllowPath = appsettings["DiagnosticLogAllowPath"]?.Split(',');
            config.ForbbidenPath = appsettings["DiagnosticLogForbbidenPath"]?.Split(',');
            DiagnosticLogConfig.Config = config;
            FilterRule.AllowPath = config.AllowPath;
            FilterRule.ForbbidenPath = config.ForbbidenPath;
            PostHelper.Init(config.CollectServerAddress, config.ClientName, config.EnvName, config.TimeoutBySecond);
            DynamicModuleUtility.RegisterModule(typeof(LogHttpModule));
            DiagnosticListener.AllListeners.Subscribe(new TracingDiagnosticProcessorObserver(new ITracingDiagnosticProcessor[] { new DapperExDiagnosticProcessor() }));
        }
    }
}
