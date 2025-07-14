namespace VetClinicManager.Services;

public class FileService : IFileService
{
    private readonly IWebHostEnvironment _webHostEnvironment;

    public FileService(IWebHostEnvironment webHostEnvironment)
    {
        _webHostEnvironment = webHostEnvironment ?? throw new ArgumentNullException(nameof(webHostEnvironment));
    }

    // Saves an uploaded file to specified subfolder in wwwroot and returns relative URL
    public async Task<string?> SaveFileAsync(IFormFile file, string subfolder)
    {
        if (file == null || file.Length == 0) return null;

        // Create target directory if it doesn't exist
        var uploadPath = Path.Combine(_webHostEnvironment.WebRootPath, subfolder);
        if (!Directory.Exists(uploadPath))
        {
            Directory.CreateDirectory(uploadPath);
        }

        // Generate unique filename and save the file
        var uniqueFileName = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
        var filePath = Path.Combine(uploadPath, uniqueFileName);

        await using (var fileStream = new FileStream(filePath, FileMode.Create))
        {
            await file.CopyToAsync(fileStream);
        }

        return $"/{subfolder.Replace('\\', '/')}/{uniqueFileName}";
    }

    // Deletes file from wwwroot if it exists
    public void DeleteFile(string? fileUrl)
    {
        if (string.IsNullOrEmpty(fileUrl)) return;
        
        var filePath = Path.Combine(_webHostEnvironment.WebRootPath, fileUrl.TrimStart('/'));
        if (File.Exists(filePath))
        {
            File.Delete(filePath);
        }
    }
}