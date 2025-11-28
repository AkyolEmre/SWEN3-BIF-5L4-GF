namespace DMS.Domain.Entities
{
    public class Document
    {
        public int Id { get; set; }
        public string FileName { get; set; } = string.Empty;
        public string ContentType { get; set; } = string.Empty;

        // Alt: Data als Byte[] (Sprint 1-3)
        public byte[]? Data { get; set; }

        // NEU: MinIO Object Name (Sprint 4)
        public string? MinIOObjectName { get; set; }

        public DateTime UploadedAt { get; set; }
        public DocumentStatus Status { get; set; } = DocumentStatus.PendingOcr;
    }

}