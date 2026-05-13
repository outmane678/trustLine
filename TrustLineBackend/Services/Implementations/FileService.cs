using AnonymousComplaintsAPI.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using System.IO.Compression;

namespace AnonymousComplaintsAPI.Services.Implementations;

/// <summary>
/// Implementation of file operations service
/// </summary>
public class FileService : IFileService
{
    private readonly IWebHostEnvironment _environment;
    private readonly ILogger<FileService> _logger;

    public FileService(IWebHostEnvironment environment, ILogger<FileService> logger)
    {
        _environment = environment;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<string> SaveFileAsync(IFormFile file, string folder)
    {
        try
        {
            if (file == null || file.Length == 0)
            {
                throw new ArgumentException("File is empty or null", nameof(file));
            }

            // Create folder if it doesn't exist
            var folderPath = Path.Combine(_environment.WebRootPath, folder);
            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
                _logger.LogInformation("Created directory: {FolderPath}", folderPath);
            }

            // Generate unique filename
            var uniqueFileName = GetUniqueFileName(file.FileName);
            var filePath = Path.Combine(folderPath, uniqueFileName);

            // Save file to disk
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            _logger.LogInformation("File saved successfully: {FileName} -> {FilePath}", file.FileName, filePath);

            // Return relative path (relative to wwwroot)
            return Path.Combine(folder, uniqueFileName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving file: {FileName}", file.FileName);
            throw new IOException($"Failed to save file: {file.FileName}", ex);
        }
    }

    /// <inheritdoc/>
    public async Task DeleteFileAsync(string filePath)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(filePath))
            {
                return;
            }

            var fullPath = Path.Combine(_environment.WebRootPath, filePath);

            if (File.Exists(fullPath))
            {
                await Task.Run(() => File.Delete(fullPath));
                _logger.LogInformation("File deleted successfully: {FilePath}", fullPath);
            }
            else
            {
                _logger.LogWarning("File not found for deletion: {FilePath}", fullPath);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting file: {FilePath}", filePath);
            throw new IOException($"Failed to delete file: {filePath}", ex);
        }
    }

    /// <inheritdoc/>
    public async Task<Stream> CreateZipFromFilesAsync(List<string> filePaths, string zipFileName)
    {
        try
        {
            var memoryStream = new MemoryStream();

            using (var archive = new ZipArchive(memoryStream, ZipArchiveMode.Create, true))
            {
                foreach (var filePath in filePaths)
                {
                    var fullPath = Path.Combine(_environment.WebRootPath, filePath);

                    if (!File.Exists(fullPath))
                    {
                        _logger.LogWarning("File not found in ZIP creation: {FilePath}", fullPath);
                        continue;
                    }

                    // Add file to ZIP
                    var fileName = Path.GetFileName(filePath);
                    var entry = archive.CreateEntry(fileName);

                    using var entryStream = entry.Open();
                    using var fileStream = File.OpenRead(fullPath);
                    await fileStream.CopyToAsync(entryStream);
                }
            }

            memoryStream.Position = 0;
            _logger.LogInformation("ZIP archive created successfully: {ZipFileName}, {FileCount} files", zipFileName, filePaths.Count);

            return memoryStream;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating ZIP archive: {ZipFileName}", zipFileName);
            throw new IOException($"Failed to create ZIP archive: {zipFileName}", ex);
        }
    }

    /// <inheritdoc/>
    public bool ValidateFile(IFormFile file, long maxSizeBytes, string[] allowedExtensions)
    {
        if (file == null || file.Length == 0)
        {
            _logger.LogWarning("File validation failed: File is empty or null");
            return false;
        }

        // Check file size
        if (file.Length > maxSizeBytes)
        {
            _logger.LogWarning("File validation failed: File size {FileSize} exceeds maximum {MaxSize}",
                file.Length, maxSizeBytes);
            return false;
        }

        // Check file extension
        var fileExtension = Path.GetExtension(file.FileName)?.ToLowerInvariant();
        if (string.IsNullOrEmpty(fileExtension) || !allowedExtensions.Contains(fileExtension))
        {
            _logger.LogWarning("File validation failed: Extension {Extension} not allowed", fileExtension);
            return false;
        }

        // Validate MIME type matches extension (security check)
        if (!ValidateMimeType(file, fileExtension))
        {
            _logger.LogWarning("File validation failed: MIME type does not match extension for {FileName}", file.FileName);
            return false;
        }

        // Read file header to verify actual file type (magic numbers check)
        if (!ValidateFileSignature(file, fileExtension))
        {
            _logger.LogWarning("File validation failed: File signature does not match extension for {FileName}", file.FileName);
            return false;
        }

        return true;
    }

    /// <summary>
    /// Validates that the MIME type matches the file extension
    /// </summary>
    private bool ValidateMimeType(IFormFile file, string extension)
    {
        var allowedMimeTypes = new Dictionary<string, string[]>
        {
            // Images
            { ".jpg", new[] { "image/jpeg", "image/jpg" } },
            { ".jpeg", new[] { "image/jpeg", "image/jpg" } },
            { ".png", new[] { "image/png" } },
            { ".gif", new[] { "image/gif" } },
            { ".bmp", new[] { "image/bmp" } },
            { ".webp", new[] { "image/webp" } },

            // Videos
            { ".mp4", new[] { "video/mp4" } },
            { ".avi", new[] { "video/x-msvideo", "video/avi" } },
            { ".mov", new[] { "video/quicktime" } },
            { ".wmv", new[] { "video/x-ms-wmv" } },
            { ".webm", new[] { "video/webm" } },
            { ".mkv", new[] { "video/x-matroska" } },

            // Documents
            { ".pdf", new[] { "application/pdf" } },
            { ".doc", new[] { "application/msword" } },
            { ".docx", new[] { "application/vnd.openxmlformats-officedocument.wordprocessingml.document" } },
            { ".xls", new[] { "application/vnd.ms-excel" } },
            { ".xlsx", new[] { "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet" } },
            { ".ppt", new[] { "application/vnd.ms-powerpoint" } },
            { ".pptx", new[] { "application/vnd.openxmlformats-officedocument.presentationml.presentation" } }
        };

        if (!allowedMimeTypes.ContainsKey(extension))
        {
            return false;
        }

        var contentType = file.ContentType?.ToLowerInvariant();
        if (string.IsNullOrEmpty(contentType))
        {
            return false;
        }

        return allowedMimeTypes[extension].Contains(contentType);
    }

    /// <summary>
    /// Validates file signature (magic numbers) to prevent file type spoofing
    /// </summary>
    private bool ValidateFileSignature(IFormFile file, string extension)
    {
        var fileSignatures = new Dictionary<string, List<byte[]>>
        {
            // Images
            { ".jpg", new List<byte[]> { new byte[] { 0xFF, 0xD8, 0xFF } } },
            { ".jpeg", new List<byte[]> { new byte[] { 0xFF, 0xD8, 0xFF } } },
            { ".png", new List<byte[]> { new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A } } },
            { ".gif", new List<byte[]> { new byte[] { 0x47, 0x49, 0x46, 0x38 } } },
            { ".bmp", new List<byte[]> { new byte[] { 0x42, 0x4D } } },
            { ".webp", new List<byte[]> { new byte[] { 0x52, 0x49, 0x46, 0x46 } } },

            // Videos
            { ".mp4", new List<byte[]> { new byte[] { 0x00, 0x00, 0x00 }, new byte[] { 0x66, 0x74, 0x79, 0x70 } } },
            { ".avi", new List<byte[]> { new byte[] { 0x52, 0x49, 0x46, 0x46 } } },
            { ".mov", new List<byte[]> { new byte[] { 0x00, 0x00, 0x00 } } },
            { ".wmv", new List<byte[]> { new byte[] { 0x30, 0x26, 0xB2, 0x75, 0x8E, 0x66, 0xCF, 0x11 } } },
            { ".webm", new List<byte[]> { new byte[] { 0x1A, 0x45, 0xDF, 0xA3 } } },
            { ".mkv", new List<byte[]> { new byte[] { 0x1A, 0x45, 0xDF, 0xA3 } } },

            // Documents
            { ".pdf", new List<byte[]> { new byte[] { 0x25, 0x50, 0x44, 0x46 } } },
            { ".doc", new List<byte[]> { new byte[] { 0xD0, 0xCF, 0x11, 0xE0, 0xA1, 0xB1, 0x1A, 0xE1 } } },
            { ".docx", new List<byte[]> { new byte[] { 0x50, 0x4B, 0x03, 0x04 } } },
            { ".xls", new List<byte[]> { new byte[] { 0xD0, 0xCF, 0x11, 0xE0, 0xA1, 0xB1, 0x1A, 0xE1 } } },
            { ".xlsx", new List<byte[]> { new byte[] { 0x50, 0x4B, 0x03, 0x04 } } },
            { ".ppt", new List<byte[]> { new byte[] { 0xD0, 0xCF, 0x11, 0xE0, 0xA1, 0xB1, 0x1A, 0xE1 } } },
            { ".pptx", new List<byte[]> { new byte[] { 0x50, 0x4B, 0x03, 0x04 } } }
        };

        if (!fileSignatures.ContainsKey(extension))
        {
            return false;
        }

        try
        {
            using var reader = new BinaryReader(file.OpenReadStream());
            var signatures = fileSignatures[extension];
            var headerBytes = reader.ReadBytes(signatures.Max(m => m.Length));

            // Reset stream position for later use
            file.OpenReadStream().Position = 0;

            return signatures.Any(signature =>
                headerBytes.Take(signature.Length).SequenceEqual(signature));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error reading file signature for {FileName}", file.FileName);
            return false;
        }
    }

    /// <inheritdoc/>
    public string GetUniqueFileName(string originalFileName)
    {
        var fileExtension = Path.GetExtension(originalFileName);
        var fileNameWithoutExtension = Path.GetFileNameWithoutExtension(originalFileName);

        // Generate unique filename with GUID prefix
        return $"{Guid.NewGuid()}_{fileNameWithoutExtension}{fileExtension}";
    }

    /// <inheritdoc/>
    public bool FileExists(string filePath)
    {
        if (string.IsNullOrWhiteSpace(filePath))
        {
            return false;
        }

        var fullPath = Path.Combine(_environment.WebRootPath, filePath);
        return File.Exists(fullPath);
    }
}
