using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMS.Domain.Entities
{
    public class Document
    {
        public int Id { get; set; }
        public string FileName { get; set; }
        public string ContentType { get; set; }
        public byte[] Data { get; set; }  // Or use string for path if storing files externally
        public DateTime UploadedAt { get; set; }
        // Add more fields like Tags, Summary later in other sprints
    }
}