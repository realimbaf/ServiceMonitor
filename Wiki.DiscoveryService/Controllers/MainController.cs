using System.Web.Mvc;

namespace Wiki.DiscoveryService.Controllers
{
    
    public class MainController : Controller
    {
        public ViewResult StartPage()
        {
            return View();
        }
        [HttpGet]
        [Authorize]
        public ViewResult Index()
        {
            return View();
        }
    }
}