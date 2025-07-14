namespace VetClinicManager.Services;

public interface IFileService
{
    // Saves a file to specified subfolder and returns its URL path
    Task<string?> SaveFileAsync(IFormFile file, string subfolder);
    
    // Deletes file at given URL if it exists
    void DeleteFile(string? fileUrl);
}