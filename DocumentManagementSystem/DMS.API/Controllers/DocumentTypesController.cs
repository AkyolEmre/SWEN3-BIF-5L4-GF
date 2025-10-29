using Microsoft.AspNetCore.Mvc;

namespace DMS.API.Controllers
{
    [Route("api/document_types")]
    [ApiController]
    public class DocumentTypesController : ControllerBase
    {
        [HttpGet]
        public ActionResult GetAll()
        {
            return Ok(new { count = 0, results = new List<object>() });
        }
    }
}