using Microsoft.AspNetCore.Mvc;

namespace DMS.API.Controllers
{
    [Route("api")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        [HttpPost("token")]
        public ActionResult PostToken([FromBody] object credentials)
        {
            // Fake Token zurückgeben
            return Ok(new { token = "fake-jwt-token-for-dev" });
        }
    }
}