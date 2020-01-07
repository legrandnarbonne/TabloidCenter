using System.Web.Mvc;
using TabloidCenter.App_Start;
using TabloidCenter.Classes;
using TabloidCenter.Classes.Models;

namespace TabloidCenter.Controllers
{

    public class HomeController : Controller
    {


        public ActionResult Index()
        {
            if (Session["ViewModel"] == null)
            {
                SimpleLogger.SimpleLog.Log($"new login");
                var login = User.Identity.Name;

                SimpleLogger.SimpleLog.Log($"new login {login}");

                var userName = login.Split('\\')[1];

                var model = new TilesModel();

                foreach (SiteConfig cfg in AppConfig.SitesConfigs)
                {
                    if (cfg.CheckAuth(userName))
                        model.AllowedSiteCollection.Add(cfg);
                    else
                        model.DisallowedSiteCollection.Add(cfg);
                }

                Session["ViewModel"] = model;
            }

            return View(Session["ViewModel"]);
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }


    }
}