using System;

namespace DMS.Domain.Entities
{
    public class Document
    {
        public int Id { get; set; }
        public string FileName { get; set; } = string.Empty;
        public string ContentType { get; set; } = string.Empty;
        public byte[] Data { get; set; } = Array.Empty<byte>();
        public DateTime UploadedAt { get; set; }
        public DocumentStatus Status { get; set; }
    }
}