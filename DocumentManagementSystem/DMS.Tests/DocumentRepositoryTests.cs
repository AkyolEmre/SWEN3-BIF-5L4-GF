using Microsoft.EntityFrameworkCore;
using Xunit;
using DMS.DAL;
using DMS.DAL.Repositories;
using DMS.Domain.Entities;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;

namespace DMS.Tests
{
    public class DocumentRepositoryTests
    {
        private ApplicationDbContext GetInMemoryDbContext()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: "TestDb_" + Guid.NewGuid().ToString())  // Unique DB per test to avoid conflicts
                .Options;
            return new ApplicationDbContext(options);
        }

        [Fact]
        public async Task AddAsync_AddsDocumentAndAssignsId()
        {
            // Arrange
            using var context = GetInMemoryDbContext();
            var repo = new DocumentRepository(context);
            var doc = new Document
            {
                FileName = "test.pdf",
                ContentType = "application/pdf",
                Data = new byte[] { 0x25, 0x50, 0x44, 0x46 },  // Dummy data
                UploadedAt = DateTime.UtcNow
            };

            // Act
            await repo.AddAsync(doc);

            // Assert
            var result = await repo.GetByIdAsync(doc.Id);
            Assert.NotNull(result);
            Assert.Equal("test.pdf", result.FileName);
            Assert.True(doc.Id > 0);  // Ensure ID was auto-generated
        }

        [Fact]
        public async Task GetByIdAsync_ReturnsDocument_WhenExists()
        {
            // Arrange
            using var context = GetInMemoryDbContext();
            var repo = new DocumentRepository(context);
            var doc = new Document
            {
                FileName = "existing.pdf",
                ContentType = "application/pdf",
                Data = new byte[] { 0x25, 0x50, 0x44, 0x46 },
                UploadedAt = DateTime.UtcNow
            };
            await repo.AddAsync(doc);

            // Act
            var result = await repo.GetByIdAsync(doc.Id);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(doc.FileName, result.FileName);
        }

        [Fact]
        public async Task GetByIdAsync_ReturnsNull_WhenNotExists()
        {
            // Arrange
            using var context = GetInMemoryDbContext();
            var repo = new DocumentRepository(context);

            // Act
            var result = await repo.GetByIdAsync(999);  // Non-existent ID

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task GetAllAsync_ReturnsAllDocuments()
        {
            // Arrange
            using var context = GetInMemoryDbContext();
            var repo = new DocumentRepository(context);
            var doc1 = new Document
            {
                FileName = "doc1.pdf",
                ContentType = "application/pdf",
                Data = new byte[] { 0x25, 0x50, 0x44, 0x46 },
                UploadedAt = DateTime.UtcNow
            };
            var doc2 = new Document
            {
                FileName = "doc2.pdf",
                ContentType = "application/pdf",
                Data = new byte[] { 0x25, 0x50, 0x44, 0x46 },
                UploadedAt = DateTime.UtcNow
            };
            await repo.AddAsync(doc1);
            await repo.AddAsync(doc2);

            // Act
            var results = await repo.GetAllAsync();

            // Assert
            Assert.Equal(2, results.Count());
            Assert.Contains(results, d => d.FileName == "doc1.pdf");
            Assert.Contains(results, d => d.FileName == "doc2.pdf");
        }

        [Fact]
        public async Task GetAllAsync_ReturnsEmpty_WhenNoDocuments()
        {
            // Arrange
            using var context = GetInMemoryDbContext();
            var repo = new DocumentRepository(context);

            // Act
            var results = await repo.GetAllAsync();

            // Assert
            Assert.Empty(results);
        }

        [Fact]
        public async Task UpdateAsync_UpdatesDocument()
        {
            // Arrange
            using var context = GetInMemoryDbContext();
            var repo = new DocumentRepository(context);
            var doc = new Document
            {
                FileName = "original.pdf",
                ContentType = "application/pdf",
                Data = new byte[] { 0x25, 0x50, 0x44, 0x46 },
                UploadedAt = DateTime.UtcNow
            };
            await repo.AddAsync(doc);

            // Modify the document
            doc.FileName = "updated.pdf";

            // Act
            await repo.UpdateAsync(doc);

            // Assert
            var updatedDoc = await repo.GetByIdAsync(doc.Id);
            Assert.NotNull(updatedDoc);
            Assert.Equal("updated.pdf", updatedDoc.FileName);
        }

        [Fact]
        public async Task DeleteAsync_RemovesDocument_WhenExists()
        {
            // Arrange
            using var context = GetInMemoryDbContext();
            var repo = new DocumentRepository(context);
            var doc = new Document
            {
                FileName = "toDelete.pdf",
                ContentType = "application/pdf",
                Data = new byte[] { 0x25, 0x50, 0x44, 0x46 },
                UploadedAt = DateTime.UtcNow
            };
            await repo.AddAsync(doc);

            // Act
            await repo.DeleteAsync(doc.Id);

            // Assert
            var result = await repo.GetByIdAsync(doc.Id);
            Assert.Null(result);
        }

        [Fact]
        public async Task DeleteAsync_DoesNothing_WhenNotExists()
        {
            // Arrange
            using var context = GetInMemoryDbContext();
            var repo = new DocumentRepository(context);

            // Act
            await repo.DeleteAsync(999);  // Should not throw any exception

            // Assert
            // No exception is thrown, and since the DB is empty, no further assertions needed
            // To make it more robust, we can add a document and verify it's unaffected
            var doc = new Document
            {
                FileName = "unaffected.pdf",
                ContentType = "application/pdf",
                Data = new byte[] { 0x25, 0x50, 0x44, 0x46 },
                UploadedAt = DateTime.UtcNow
            };
            await repo.AddAsync(doc);

            await repo.DeleteAsync(999);  // Delete non-existent

            var remainingDocs = await repo.GetAllAsync();
            Assert.Single(remainingDocs);  // Still one document
            Assert.Equal("unaffected.pdf", remainingDocs.First().FileName);
        }
    }
}