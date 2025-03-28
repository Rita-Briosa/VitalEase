using Microsoft.AspNetCore.Mvc;

namespace VitalEase.Server.Controllers
{
    public class MapsController : Controller
    {
        private readonly IConfiguration _configuration;

        public MapsController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public IActionResult Index()
        {
            ViewBag.GoogleMapsApiKey = _configuration["GoogleMaps:ApiKey"];
            return View();
        }
    }
}
