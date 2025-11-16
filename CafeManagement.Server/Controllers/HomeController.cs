using Microsoft.AspNetCore.Mvc;

namespace CafeManagement.Server.Controllers
{
    [ApiController]
    [Route("")]
    public class HomeController : ControllerBase
    {
        [HttpGet]
        public IActionResult Get()
        {
            return Ok(new
            {
                Message = "Cafe Management System API",
                Version = "1.0.0",
                Endpoints = new
                {
                    Health = "/health",
                    Swagger = "/swagger",
                    Auth = "/api/auth",
                    Clients = "/api/clients",
                    Sessions = "/api/sessions",
                    SignalR_Hub = "/hub/cafemanagement"
                }
            });
        }

        [HttpGet("health")]
        public IActionResult Health()
        {
            return Ok(new
            {
                Status = "Healthy",
                Timestamp = DateTime.UtcNow
            });
        }
    }
}