namespace PortalBeritaApp.Services
{
    public class FileService : IFileService
    {
        private readonly IWebHostEnvironment _env;
        private readonly ILogger<FileService> _logger;
        private readonly string[] _allowedExtensions = { ".jpg", ".jpeg", ".png", ".webp" };
        private const long MaxFileSize = 5 * 1024 * 1024; // 5 MB

        public FileService(IWebHostEnvironment env, ILogger<FileService> logger)
        {
            _env = env;
            _logger = logger;
        }

        public async Task<string?> UploadImageAsync(IFormFile? file, string subFolder = "articles")
        {
            if (file == null || file.Length == 0)
                return null;

            if (file.Length > MaxFileSize)
            {
                throw new InvalidOperationException("Ukuran file gambar maksimal 5MB.");
            }

            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!_allowedExtensions.Contains(extension))
            {
                throw new InvalidOperationException("Format gambar yang didukung hanya JPG, JPEG, PNG, dan WEBP.");
            }

            // Ensure directory exists
            var uploadsDir = Path.Combine(_env.WebRootPath, "uploads", subFolder);
            if (!Directory.Exists(uploadsDir))
            {
                Directory.CreateDirectory(uploadsDir);
            }

            // Unique filename: {Guid}_{timestamp}.ext
            var uniqueFileName = $"{Guid.NewGuid():N}_{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}{extension}";
            var fullPath = Path.Combine(uploadsDir, uniqueFileName);

            using (var stream = new FileStream(fullPath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            _logger.LogInformation("File uploaded successfully: {Path}", fullPath);

            // Web-friendly URL path
            return $"/uploads/{subFolder}/{uniqueFileName}";
        }

        public void DeleteFile(string? relativeFilePath)
        {
            if (string.IsNullOrWhiteSpace(relativeFilePath))
                return;

            try
            {
                // Normalize path
                var trimmed = relativeFilePath.TrimStart('/').Replace('/', Path.DirectorySeparatorChar);
                var fullPath = Path.Combine(_env.WebRootPath, trimmed);

                if (File.Exists(fullPath))
                {
                    File.Delete(fullPath);
                    _logger.LogInformation("File deleted: {Path}", fullPath);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to delete file: {Path}", relativeFilePath);
            }
        }
    }
}
