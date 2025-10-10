using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using DMS.Domain.Entities;

namespace DMS.DAL.Repositories
{
    public interface IDocumentRepository
    {
        Task<Document> GetByIdAsync(int id);
        Task<IEnumerable<Document>> GetAllAsync();
        Task AddAsync(Document document);
        Task UpdateAsync(Document document);
        Task DeleteAsync(int id);
    }
}