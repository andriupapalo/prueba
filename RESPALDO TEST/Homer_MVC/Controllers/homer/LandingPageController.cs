using System.Web.Mvc;

namespace Homer_MVC.Controllers
{
    public class LandingPageController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }
    }
}