using Microsoft.Owin;
using Owin;
using System;
using System.Web;
using TabloidCenter.App_Start;
using TabloidCenter.Classes;

[assembly: OwinStartupAttribute(typeof(TabloidCenter.Startup))]
namespace TabloidCenter
{
    public partial class Startup
    {

        public void Configuration(IAppBuilder app)
        {

            var logPath = HttpContext.Current.Server.MapPath("logs");
            var configPath = HttpContext.Current.Server.MapPath("tabloidcenter.config");

            SimpleLogger.SimpleLog.SetLogFile(logPath);
            SimpleLogger.SimpleLog.Log("Application started");

            
            AppConfig.CurrentConfig = Config.Load(configPath);

            SimpleLogger.SimpleLog.Log($"Analysing config file for {AppConfig.CurrentConfig.Paths.Length} paths");

            AppConfig.SitesConfigs = new System.Collections.Generic.List<SiteConfig>();

            //buil tabloid list
            foreach (string path in AppConfig.CurrentConfig.Paths)
            {
                try
                {                    
                    AppConfig.SitesConfigs.Add(SiteConfig.Load(path));
                }
                catch(Exception e)
                {
                    SimpleLogger.SimpleLog.Log(e.ToString());
                    continue;
                }
            }

            SimpleLogger.SimpleLog.Log($"{AppConfig.CurrentConfig.Paths.Length} sites configs loaded");
        }
    }
}
