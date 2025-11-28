using Microsoft.Extensions.Configuration;
using Minio;
using Minio.DataModel.Args;

namespace DMS.API.Services  // oder DMS.OCRWorker → je nach Projekt
{
    public interface IMinIOService
    {
        Task<string> UploadFileAsync(Stream fileStream, string fileName, string contentType);
        Task<Stream> GetFileAsync(string objectName);
    }

    public class MinIOService : IMinIOService
    {
        private readonly IMinioClient _minio;
        private readonly string _bucketName;

        public MinIOService(IConfiguration config)
        {
            _bucketName = config["MinIO:BucketName"] ?? "documents";

            _minio = new MinioClient()
                .WithEndpoint(config["MinIO:Endpoint"] ?? "minio:9000")
                .WithCredentials(
                    config["MinIO:AccessKey"] ?? "minioadmin",
                    config["MinIO:SecretKey"] ?? "minioadmin")
                .WithSSL(false)
                .Build();

            // Bucket anlegen, falls nicht vorhanden
            EnsureBucketExistsAsync().GetAwaiter().GetResult();
        }

        private async Task EnsureBucketExistsAsync()
        {
            var beArgs = new BucketExistsArgs().WithBucket(_bucketName);
            bool exists = await _minio.BucketExistsAsync(beArgs);
            if (!exists)
            {
                var mbArgs = new MakeBucketArgs().WithBucket(_bucketName);
                await _minio.MakeBucketAsync(mbArgs);
            }
        }

        public async Task<string> UploadFileAsync(Stream fileStream, string fileName, string contentType)
        {
            var objectName = $"{Guid.NewGuid():N}_{fileName}";

            var putArgs = new PutObjectArgs()
                .WithBucket(_bucketName)
                .WithObject(objectName)
                .WithStreamData(fileStream)
                .WithObjectSize(fileStream.Length)
                .WithContentType(contentType);

            await _minio.PutObjectAsync(putArgs);
            return objectName;
        }

        public async Task<Stream> GetFileAsync(string objectName)
        {
            var memoryStream = new MemoryStream();

            var getArgs = new GetObjectArgs()
                .WithBucket(_bucketName)
                .WithObject(objectName)
                .WithCallbackStream(stream =>
                {
                    stream.CopyTo(memoryStream);
                    memoryStream.Position = 0;
                });

            await _minio.GetObjectAsync(getArgs);
            memoryStream.Position = 0;
            return memoryStream;
        }
    }
}