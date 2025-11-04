using Microsoft.AspNetCore.Mvc;

namespace ITI.Resturant.Management.MVC.Controllers
{
    public class BusinessController : Controller
    {
        [HttpGet]
        [Route("business/closed")]
        public IActionResult Closed(string opening, string closing, int happy = 0, int isOrder = 0, string returnUrl = "/")
        {
            ViewData["Opening"] = opening;
            ViewData["Closing"] = closing;
            ViewData["IsHappyHour"] = happy == 1;
            ViewData["IsOrder"] = isOrder == 1;
            ViewData["ReturnUrl"] = returnUrl;

            return View();
        }
    }
}
