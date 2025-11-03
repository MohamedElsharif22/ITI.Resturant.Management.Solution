using Microsoft.AspNetCore.Mvc;

namespace ITI.Resturant.Management.MVC.Controllers
{
    [Route("business")]
    public class BusinessController : Controller
    {
        [HttpGet("closed")]
        public IActionResult Closed(string opening, string closing, string happy, string isOrder, string returnUrl)
        {
            ViewBag.Opening = opening ?? "09:00";
            ViewBag.Closing = closing ?? "22:00";
            ViewBag.IsHappyHour = happy == "1";
            ViewBag.IsOrderAttempt = isOrder == "1";
            ViewBag.ReturnUrl = returnUrl ?? "/";
            return View();
        }

        // Optional: quick action to navigate back
        [HttpGet("back")]
        public IActionResult Back(string returnUrl = "/")
        {
            return Redirect(returnUrl);
        }
    }
}
