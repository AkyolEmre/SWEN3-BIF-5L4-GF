using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using DMS.DAL;
using DMS.DAL.Repositories;
using DMS.Domain.Entities;
using System.Collections.Generic;
using System.Linq;

namespace DMS.Tests
{
    public class DocumentRepositoryTests
    {
        private ApplicationDbContext GetInMemoryDbContext()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())  // Unique DB per test
                .Options;
            return new ApplicationDbContext(options);
        }

        [Fact]
        public async Task AddAsync_AddsDocumentAndSaves()
        {
            // Arrange
            using var context = GetInMemoryDbContext();
            var loggerMock = new Mock<ILogger<DocumentRepository>>();  // Mock ILogger
            var repository = new DocumentRepository(context, loggerMock.Object);
            var document = new Document
            {
                FileName = "test.pdf",
                ContentType = "application/pdf",
                Data = new byte[] { 1, 2, 3 },
                UploadedAt = DateTime.UtcNow
            };

            // Act
            await repository.AddAsync(document);

            // Assert
            var savedDocument = await context.Documents.FirstOrDefaultAsync(d => d.FileName == "test.pdf");
            Assert.NotNull(savedDocument);
            Assert.Equal("test.pdf", savedDocument.FileName);
        }

        [Fact]
        public async Task GetByIdAsync_ReturnsDocument_WhenExists()
        {
            // Arrange
            using var context = GetInMemoryDbContext();
            var loggerMock = new Mock<ILogger<DocumentRepository>>();
            var document = new Document { FileName = "existing.pdf" };
            await context.Documents.AddAsync(document);
            await context.SaveChangesAsync();
            var repository = new DocumentRepository(context, loggerMock.Object);

            // Act
            var result = await repository.GetByIdAsync(document.Id);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("existing.pdf", result.FileName);
        }

        [Fact]
        public async Task GetByIdAsync_ReturnsNull_WhenNotExists()
        {
            // Arrange
            using var context = GetInMemoryDbContext();
            var loggerMock = new Mock<ILogger<DocumentRepository>>();
            var repository = new DocumentRepository(context, loggerMock.Object);

            // Act
            var result = await repository.GetByIdAsync(999);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task GetAllAsync_ReturnsAllDocuments()
        {
            // Arrange
            using var context = GetInMemoryDbContext();
            var loggerMock = new Mock<ILogger<DocumentRepository>>();
            await context.Documents.AddRangeAsync(
                new Document { FileName = "doc1.pdf" },
                new Document { FileName = "doc2.pdf" }
            );
            await context.SaveChangesAsync();
            var repository = new DocumentRepository(context, loggerMock.Object);

            // Act
            var results = await repository.GetAllAsync();

            // Assert
            Assert.Equal(2, results.Count());
        }

        [Fact]
        public async Task UpdateAsync_UpdatesDocument()
        {
            // Arrange
            using var context = GetInMemoryDbContext();
            var loggerMock = new Mock<ILogger<DocumentRepository>>();
            var document = new Document { FileName = "old.pdf" };
            await context.Documents.AddAsync(document);
            await context.SaveChangesAsync();
            var repository = new DocumentRepository(context, loggerMock.Object);
            document.FileName = "new.pdf";

            // Act
            await repository.UpdateAsync(document);

            // Assert
            var updated = await context.Documents.FindAsync(document.Id);
            Assert.Equal("new.pdf", updated.FileName);
        }

        [Fact]
        public async Task DeleteAsync_DeletesDocument_WhenExists()
        {
            // Arrange
            using var context = GetInMemoryDbContext();
            var loggerMock = new Mock<ILogger<DocumentRepository>>();
            var document = new Document { FileName = "delete.pdf" };
            await context.Documents.AddAsync(document);
            await context.SaveChangesAsync();
            var repository = new DocumentRepository(context, loggerMock.Object);

            // Act
            await repository.DeleteAsync(document.Id);

            // Assert
            var deleted = await context.Documents.FindAsync(document.Id);
            Assert.Null(deleted);
        }

        // Add more tests if needed for edge cases
    }
}