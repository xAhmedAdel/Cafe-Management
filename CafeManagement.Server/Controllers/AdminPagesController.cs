using Microsoft.AspNetCore.Mvc;

namespace CafeManagement.Server.Controllers;

public class AdminPagesController : Controller
{
    [Route("/admin")]
    public IActionResult Index()
    {
        return PhysicalFile(Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "admin", "index.html"), "text/html");
    }

    [Route("/")]
    public IActionResult Home()
    {
        return RedirectToAction("Index");
    }
}