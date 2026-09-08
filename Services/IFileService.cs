using Microsoft.AspNetCore.Http;

namespace PortalBeritaApp.Services
{
    public interface IFileService
    {
        Task<string?> UploadImageAsync(IFormFile? file, string subFolder = "articles");
        void DeleteFile(string? relativeFilePath);
    }
}
