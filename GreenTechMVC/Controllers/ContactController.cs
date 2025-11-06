using Microsoft.AspNetCore.Mvc;

namespace GreenTechMVC.Controllers
{
    public class ContactController : Controller
    {
        private readonly ILogger<ContactController> _logger;

        public ContactController(ILogger<ContactController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            ViewData["Title"] = "Liên hệ";
            return View();
        }

        [HttpPost]
        public IActionResult SendContact(
            string fullName,
            string phone,
            string title,
            string content
        )
        {
            // TODO: Implement contact form submission logic
            // For now, just return success message
            TempData["SuccessMessage"] =
                "Cảm ơn bạn đã liên hệ! Chúng tôi sẽ phản hồi sớm nhất có thể.";
            return RedirectToAction("Index");
        }
    }
}
