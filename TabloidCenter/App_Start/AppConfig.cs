

using System.Collections.Generic;
using TabloidCenter.Classes;

namespace TabloidCenter.App_Start
{
    public static class AppConfig
    {
        public static Config CurrentConfig { get; set; }

        public static List<SiteConfig> SitesConfigs { get; set; }
    }
}