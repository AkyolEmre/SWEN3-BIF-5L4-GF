namespace DMS.API.Services
{
    public interface IMinIOService
    {
        Task<string> UploadFileAsync(Stream fileStream, string fileName, string contentType);
        Task<Stream> GetFileAsync(string objectName);
    }
}