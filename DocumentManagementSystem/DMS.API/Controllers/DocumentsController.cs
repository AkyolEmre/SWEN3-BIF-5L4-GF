using Microsoft.AspNetCore.Mvc;
using DMS.DAL.Repositories;
using DMS.Domain.Entities;

namespace DMS.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DocumentsController : ControllerBase
    {
        private readonly IDocumentRepository _repository;

        public DocumentsController(IDocumentRepository repository)
        {
            _repository = repository;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Document>>> GetAll()
        {
            return Ok(await _repository.GetAllAsync());
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Document>> GetById(int id)
        {
            var doc = await _repository.GetByIdAsync(id);
            if (doc == null) return NotFound();
            return Ok(doc);
        }

        [HttpPost]
        public async Task<ActionResult> Add([FromBody] Document document)
        {
            await _repository.AddAsync(document);
            return CreatedAtAction(nameof(GetById), new { id = document.Id }, document);
        }

        [HttpPut("{id}")]
        public async Task<ActionResult> Update(int id, [FromBody] Document document)
        {
            if (id != document.Id) return BadRequest();
            await _repository.UpdateAsync(document);
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> Delete(int id)
        {
            await _repository.DeleteAsync(id);
            return NoContent();
        }
    }
}