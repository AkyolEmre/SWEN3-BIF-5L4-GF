using DMS.API.Services;   // <-- schadet nicht, falls du das Interface in einer anderen Datei hast
using Minio;                       // <-- bleibt
using Minio.DataModel.Args;        // <-- hinzu

public class MinIOService : IMinIOService
{
    private readonly IMinioClient _minio;
    private readonly string _bucketName;

    public MinIOService(IConfiguration config)
    {
        _bucketName = config["MinIO:BucketName"] ?? "documents";

        _minio = new MinioClient()
                     .WithEndpoint(config["MinIO:Endpoint"])
                     .WithCredentials(config["MinIO:AccessKey"], config["MinIO:SecretKey"])
                     .WithSSL(false)
                     .Build();

        EnsureBucketExistsAsync().Wait();
    }

    private async Task EnsureBucketExistsAsync()
    {
        bool exists = await _minio.BucketExistsAsync(
                          new BucketExistsArgs().WithBucket(_bucketName));
        if (!exists)
            await _minio.MakeBucketAsync(
                   new MakeBucketArgs().WithBucket(_bucketName));
    }

    public async Task<string> UploadFileAsync(Stream fileStream,
                                              string fileName,
                                              string contentType)
    {
        var objectName = $"{Guid.NewGuid()}_{fileName}";
        await _minio.PutObjectAsync(
            new PutObjectArgs()
                .WithBucket(_bucketName)
                .WithObject(objectName)
                .WithStreamData(fileStream)
                .WithObjectSize(fileStream.Length)
                .WithContentType(contentType));
        return objectName;
    }

    public async Task<Stream> GetFileAsync(string objectName)
    {
        var ms = new MemoryStream();
        await _minio.GetObjectAsync(
            new GetObjectArgs()
                .WithBucket(_bucketName)
                .WithObject(objectName)
                .WithCallbackStream(stream => stream.CopyTo(ms)));
        ms.Position = 0;
        return ms;
    }
}