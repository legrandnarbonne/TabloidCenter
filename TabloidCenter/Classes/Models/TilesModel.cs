
using System.Collections.Generic;


namespace TabloidCenter.Classes.Models
{
    public class TilesModel
    {
        public List<SiteConfig> AllowedSiteCollection { get; set; }
        public List<SiteConfig> DisallowedSiteCollection { get; set; }

        public TilesModel()
        {
            AllowedSiteCollection = new List<SiteConfig>();
            DisallowedSiteCollection = new List<SiteConfig>();
        }
    }
}