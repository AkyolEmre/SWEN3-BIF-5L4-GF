using Moq;
using Microsoft.EntityFrameworkCore;
using Xunit;
using DMS.DAL;
using DMS.DAL.Repositories;
using DMS.Domain.Entities;

namespace DMS.Tests
{
    public class DocumentRepositoryTests
    {
        [Fact]
        public async Task AddAsync_AddsDocument()
        {
            // Arrange: Mock DbContext
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: "TestDb")
                .Options;
            var context = new ApplicationDbContext(options);
            var repo = new DocumentRepository(context);

            var doc = new Document { FileName = "test.pdf" };

            // Act
            await repo.AddAsync(doc);

            // Assert
            var result = await repo.GetByIdAsync(doc.Id);
            Assert.NotNull(result);
            Assert.Equal("test.pdf", result.FileName);
        }

        // Add more tests for Get, Update, etc., mocking production DB
    }
}