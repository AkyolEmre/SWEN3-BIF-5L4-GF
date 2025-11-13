using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using DMS.Domain.Entities;
using DMS.DAL.Exceptions;
using DMS.Domain.Exceptions;
using Microsoft.Extensions.Logging;

namespace DMS.DAL.Repositories
{
    public class DocumentRepository : IDocumentRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<DocumentRepository> _logger;

        public DocumentRepository(ApplicationDbContext context, ILogger<DocumentRepository> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<Document> GetByIdAsync(int id)
        {
            return await _context.Documents.FindAsync(id);
        }

        public async Task<IEnumerable<Document>> GetAllAsync()
        {
            return await _context.Documents.ToListAsync();
        }
        public async Task AddAsync(Document document)
        {
            try
            {
                if (document == null) throw new DomainException("Document cannot be null");  // Beispiel für Domain-Check (könnte in Domain-Layer sein)
                await _context.Documents.AddAsync(document);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException ex)
            {
                // Low-level DB-Fehler fangen und in DalException umwandeln
                throw new DalException("Failed to save document to database", ex);
            }
            catch (Exception ex)
            {
                // Allgemeiner Fallback
                throw new DalException("Unexpected error during add operation", ex);
            }
        }

        public async Task UpdateAsync(Document document)
        {
            _context.Documents.Update(document);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var document = await GetByIdAsync(id);
            if (document != null)
            {
                _context.Documents.Remove(document);
                await _context.SaveChangesAsync();
            }
        }
    }
}