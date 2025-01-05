using PostService_module.Core.Interfaces;

public class UploadService : IUploadService
{
    private readonly IWebHostEnvironment _environment;
    private readonly string _uploadDirectory = Path.Combine("uploads", "images");

    public UploadService(IWebHostEnvironment environment)
    {
        _environment = environment;
    }

    public async Task<string> UploadImageAsync(IFormFile file)
    {
        if (file == null || file.Length == 0)
            throw new ArgumentException("Invalid file");

        // Debug logging
        Console.WriteLine($"Uploading file: {file.FileName}");
        Console.WriteLine($"Content type: {file.ContentType}");

        // file type extensions
        var allowedContentTypes = new[] { "image/jpeg", "image/png", "image/gif", "image/jpg" };
        var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };

        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();

        // Debug logging
        Console.WriteLine($"File extension: {extension}");

        // Check contect and extension
        if (!allowedContentTypes.Contains(file.ContentType.ToLower()) &&
            !allowedExtensions.Contains(extension))
        {
            throw new ArgumentException($"Invalid file type. Only JPEG, PNG and GIF are allowed. Got content type: {file.ContentType} and extension: {extension}");
        }

        try
        {
            //upload directory
            var uploadPath = Path.Combine(_environment.WebRootPath, _uploadDirectory);
            Directory.CreateDirectory(uploadPath);

            // Generate unique filename with extension
            var fileName = $"{Guid.NewGuid()}{extension}";
            var filePath = Path.Combine(uploadPath, fileName);

            Console.WriteLine($"Saving file to: {filePath}");

            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(fileStream);
            }

            var returnPath = $"/uploads/images/{fileName}";
            Console.WriteLine($"File successfully saved. Returning path: {returnPath}");
            return returnPath;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error uploading file: {ex.Message}");
            throw new Exception($"Error uploading file: {ex.Message}");
        }
    }

    public void DeleteImage(string imageUrl)
    {
        if (string.IsNullOrEmpty(imageUrl)) return;
        try
        {
            var fileName = Path.GetFileName(imageUrl);
            var filePath = Path.Combine(_environment.WebRootPath, _uploadDirectory, fileName);

            Console.WriteLine($"Attempting to delete file: {filePath}");

            if (File.Exists(filePath))
            {
                File.Delete(filePath);
                Console.WriteLine("File successfully deleted");
            }
            else
            {
                Console.WriteLine("File not found for deletion");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error deleting image: {ex.Message}");
        }
    }
}