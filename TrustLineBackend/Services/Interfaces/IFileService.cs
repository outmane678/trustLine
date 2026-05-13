using Microsoft.AspNetCore.Http;

namespace AnonymousComplaintsAPI.Services.Interfaces;

/// <summary>
/// Service for file operations (upload, delete, validation, ZIP creation)
/// </summary>
public interface IFileService
{
    /// <summary>
    /// Saves an uploaded file to the specified folder
    /// </summary>
    /// <param name="file">The file to upload</param>
    /// <param name="folder">Target folder path (relative to wwwroot)</param>
    /// <returns>The relative file path where the file was saved</returns>
    Task<string> SaveFileAsync(IFormFile file, string folder);

    /// <summary>
    /// Deletes a file from disk
    /// </summary>
    /// <param name="filePath">The file path to delete (relative to wwwroot)</param>
    Task DeleteFileAsync(string filePath);

    /// <summary>
    /// Creates a ZIP archive from multiple files
    /// </summary>
    /// <param name="filePaths">List of file paths to include in the ZIP</param>
    /// <param name="zipFileName">Name for the ZIP file</param>
    /// <returns>Stream containing the ZIP file</returns>
    Task<Stream> CreateZipFromFilesAsync(List<string> filePaths, string zipFileName);

    /// <summary>
    /// Validates file size and extension
    /// </summary>
    /// <param name="file">The file to validate</param>
    /// <param name="maxSizeBytes">Maximum allowed file size in bytes</param>
    /// <param name="allowedExtensions">Array of allowed file extensions (e.g., [".pdf", ".jpg"])</param>
    /// <returns>True if valid, false otherwise</returns>
    bool ValidateFile(IFormFile file, long maxSizeBytes, string[] allowedExtensions);

    /// <summary>
    /// Generates a unique file name with GUID prefix
    /// </summary>
    /// <param name="originalFileName">The original file name</param>
    /// <returns>Unique file name with format: {GUID}_{originalFileName}</returns>
    string GetUniqueFileName(string originalFileName);

    /// <summary>
    /// Checks if a file exists on disk
    /// </summary>
    /// <param name="filePath">The file path to check (relative to wwwroot)</param>
    /// <returns>True if file exists, false otherwise</returns>
    bool FileExists(string filePath);
}
