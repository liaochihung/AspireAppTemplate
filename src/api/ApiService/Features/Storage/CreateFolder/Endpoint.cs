using AspireAppTemplate.ApiService.Infrastructure.Storage;
using FastEndpoints;
using Microsoft.AspNetCore.Authorization;

namespace AspireAppTemplate.ApiService.Features.Storage.CreateFolder;

[HttpPost("/storage/folders")]
[AllowAnonymous] // TODO: Secure this
public class Endpoint(IStorageService storageService, ILogger<Endpoint> logger) : Endpoint<CreateFolderRequest>
{
    public override async Task HandleAsync(CreateFolderRequest req, CancellationToken ct)
    {
        var folderName = req.FolderName?.Trim();
        
        if (string.IsNullOrWhiteSpace(folderName))
        {
            ThrowError("Folder name is required");
        }

        // Normalize path: ensure it ends with / and doesn't start with /
        if (!folderName.EndsWith("/")) folderName += "/";
        if (folderName.StartsWith("/")) folderName = folderName.TrimStart('/');

        // Logic to support nested folders creation
        var fullPath = req.ParentPath;
        if (!string.IsNullOrWhiteSpace(fullPath))
        {
            if (!fullPath.EndsWith("/")) fullPath += "/";
            if (fullPath.StartsWith("/")) fullPath = fullPath.TrimStart('/');
            fullPath = Path.Combine(fullPath, folderName);
        }
        else
        {
             fullPath = folderName;
        }
        
        // Ensure path separators are forward slashes for MinIO/S3
        fullPath = fullPath.Replace("\\", "/");
        
        // Use .keep file strategy
        if (fullPath.EndsWith("/")) fullPath += ".keep";
        else fullPath += "/.keep";

        try
        {
            // Create .keep object to represent the folder
            using var emptyStream = new MemoryStream();
            await storageService.UploadAsync(fullPath, emptyStream, "application/octet-stream", ct);
            
            await SendOkAsync(ct);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to create folder {Path}", fullPath);
            ThrowError($"Failed to create folder: {ex.Message}");
        }
    }
}

public class CreateFolderRequest
{
    public string FolderName { get; set; } = string.Empty;
    public string? ParentPath { get; set; }
}
