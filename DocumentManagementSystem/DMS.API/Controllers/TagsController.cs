using Microsoft.AspNetCore.Mvc;

namespace DMS.API.Controllers
{
    [Route("api/tags")]
    [ApiController]
    public class TagsController : ControllerBase
    {
        [HttpGet]
        public ActionResult GetAll()
        {
            return Ok(new { count = 0, results = new List<object>() });
        }
    }
}