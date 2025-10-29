using Microsoft.AspNetCore.Mvc;
using DMS.DAL.Repositories;
using DMS.Domain.Entities;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DMS.API.Controllers
{
    [Route("api/documents")]
    [ApiController]
    public class DocumentsController : ControllerBase
    {
        private readonly IDocumentRepository _repository;

        public DocumentsController(IDocumentRepository repository)
        {
            _repository = repository;
        }

        [HttpGet]
        public async Task<ActionResult> GetAll([FromQuery] int page = 1, [FromQuery] int page_size = 25, [FromQuery] string? query = null)
        {
            var allDocs = await _repository.GetAllAsync();

            if (!string.IsNullOrEmpty(query))
            {
                allDocs = allDocs.Where(d => d.FileName.Contains(query, StringComparison.OrdinalIgnoreCase)).ToList();
            }

            var count = allDocs.Count();
            var results = allDocs.Skip((page - 1) * page_size).Take(page_size).ToList();

            var nextUrl = (page * page_size < count) ? $"?page={page + 1}&page_size={page_size}" : null;
            var previousUrl = (page > 1) ? $"?page={page - 1}&page_size={page_size}" : null;

            return Ok(new
            {
                count,
                next = nextUrl,
                previous = previousUrl,
                results
            });
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Document>> GetById(int id)
        {
            var doc = await _repository.GetByIdAsync(id);
            if (doc == null) return NotFound();
            return Ok(doc);
        }

        [HttpPost]
        public async Task<ActionResult> PostDocument(IFormFile? document = null, [FromForm] string? title = null, [FromForm] DateTime? created = null)
        {
            if (document == null) return BadRequest("No document provided.");

            using var memoryStream = new MemoryStream();
            await document.CopyToAsync(memoryStream);

            var newDoc = new Document
            {
                FileName = title ?? document.FileName,
                ContentType = document.ContentType,
                Data = memoryStream.ToArray(),
                UploadedAt = created ?? DateTime.UtcNow
            };

            await _repository.AddAsync(newDoc);

            var taskId = Guid.NewGuid().ToString();
            return CreatedAtAction(nameof(GetById), new { id = newDoc.Id }, new { task_id = taskId, id = newDoc.Id });
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

        // Placeholder for bulk_edit (implement in later sprint if needed)
        [HttpPost("bulk_edit")]
        public ActionResult BulkEdit([FromBody] object payload)
        {
            // TODO: Implement bulk operations
            return Ok();
        }
    }
}