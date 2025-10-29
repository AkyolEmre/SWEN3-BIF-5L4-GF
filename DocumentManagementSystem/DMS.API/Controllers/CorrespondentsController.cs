using Microsoft.AspNetCore.Mvc;

namespace DMS.API.Controllers
{
    [Route("api/correspondents")]
    [ApiController]
    public class CorrespondentsController : ControllerBase
    {
        [HttpGet]
        public ActionResult GetAll()
        {
            // Leere Liste zurückgeben, um 404 zu vermeiden
            return Ok(new { count = 0, results = new List<object>() });
        }

        // Später: Füge POST, PUT, etc. hinzu, wenn implementiert
    }
}