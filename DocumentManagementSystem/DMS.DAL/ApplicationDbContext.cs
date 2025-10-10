using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using DMS.Domain.Entities;
using System.Reflection.Emit;
using System.Reflection.Metadata;
using MyDocument = DMS.Domain.Entities.Document;

namespace DMS.DAL
{
    public class ApplicationDbContext : DbContext
    {
        public DbSet<DMS.Domain.Entities.Document> Documents { get; set; }

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Configure mappings if needed
        }
    }
}