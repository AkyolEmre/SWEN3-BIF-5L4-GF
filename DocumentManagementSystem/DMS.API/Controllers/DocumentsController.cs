using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using DMS.API.Services;
using DMS.DAL.Repositories;
using DMS.Domain.Entities;
using System;
using System.IO;
using System.Threading.Tasks;
using DMS.Common.Exceptions;

namespace DMS.API.Controllers
{
    [Route("api/documents")]
    [ApiController]
    public class DocumentsController : ControllerBase
    {
        private readonly IDocumentRepository _repo;
        private readonly IMessageProducer _messageProducer;
        private readonly ILogger<DocumentsController> _logger;

        public DocumentsController(IDocumentRepository repo, IMessageProducer messageProducer, ILogger<DocumentsController> logger)
        {
            _repo = repo;
            _messageProducer = messageProducer;
            _logger = logger;
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<Document>> GetById(int id)
        {
            var doc = await _repo.GetByIdAsync(id);
            return doc is null ? NotFound() : Ok(doc);
        }

        [HttpPost]
        public async Task<ActionResult> PostDocument(IFormFile? document = null, [FromForm] string? title = null)
        {
            if (document is null || document.Length == 0)
                return BadRequest("No file uploaded.");

            await using var ms = new MemoryStream();
            await document.CopyToAsync(ms);

            var doc = new Document
            {
                FileName = title ?? document.FileName,
                ContentType = document.ContentType,
                Data = ms.ToArray(),
                UploadedAt = DateTime.UtcNow,
                Status = DocumentStatus.PendingOcr
            };

            await _repo.AddAsync(doc);
            _logger.LogInformation("Document {DocumentId} saved.", doc.Id);

            try
            {
                await _messageProducer.SendMessageAsync(doc.Id.ToString());
                _logger.LogInformation("Published OCR job {DocumentId}", doc.Id);
            }
            catch (QueueException ex)
            {
                _logger.LogError(ex, "Queue publish failed for {DocumentId}", doc.Id);
                return StatusCode(503, "OCR queue unavailable");
            }

            return CreatedAtAction(nameof(GetById), new { id = doc.Id }, new { id = doc.Id, status = doc.Status });
        }
    }
}