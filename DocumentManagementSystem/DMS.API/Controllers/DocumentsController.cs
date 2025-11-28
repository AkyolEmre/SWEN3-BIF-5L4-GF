using DMS.API.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
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
        private readonly IMinIOService _minioService;

        public DocumentsController(
            IDocumentRepository repo,
            IMessageProducer messageProducer,
            ILogger<DocumentsController> logger,
            IMinIOService minioService)
        {
            _repo = repo;
            _messageProducer = messageProducer;
            _logger = logger;
            _minioService = minioService;
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<Document>> GetById(int id)
        {
            var doc = await _repo.GetByIdAsync(id);
            return doc is null ? NotFound() : Ok(doc);
        }

        // SPRINT 4 UPLOAD
        [HttpPost("upload")]
        public async Task<ActionResult> Upload(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest("No file uploaded.");

            // 1. MinIO Upload
            await using var stream = file.OpenReadStream();
            var objectName = await _minioService.UploadFileAsync(stream, file.FileName, file.ContentType);

            // 2. DB Eintrag
            var document = new Document
            {
                FileName = file.FileName,
                ContentType = file.ContentType,
                MinIOObjectName = objectName,
                UploadedAt = DateTime.UtcNow,
                Status = DocumentStatus.PendingOcr
            };

            await _repo.AddAsync(document);

            // 3. RabbitMQ Nachricht (JSON Format passend zum Worker)
            var message = new { DocumentId = document.Id, ObjectName = objectName };
            var jsonMessage = System.Text.Json.JsonSerializer.Serialize(message);

            await _messageProducer.SendMessageAsync(jsonMessage);

            _logger.LogInformation("Document {Id} uploaded and queued.", document.Id);

            return CreatedAtAction(nameof(GetById), new { id = document.Id }, document);
        }
    }
}