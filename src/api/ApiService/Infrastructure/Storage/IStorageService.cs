using AspireAppTemplate.ApiService.Infrastructure.Storage;
using Microsoft.AspNetCore.Http;

namespace AspireAppTemplate.ApiService.Infrastructure.Storage;

public record StorageFile(string Name, long Size);

public interface IStorageService
{
    Task<Uri> UploadAsync<T>(IFormFile file, FileType supportedFileType, CancellationToken cancellationToken = default)
    where T : class;

    Task<Uri> UploadAsync(string objectName, Stream stream, string contentType, CancellationToken cancellationToken = default);

    Task<Stream> GetFileAsync(string objectName, CancellationToken cancellationToken = default);
    
    Task<List<StorageFile>> ListFilesAsync(CancellationToken cancellationToken = default);

    Task RemoveAsync(string objectName, CancellationToken cancellationToken = default);
}
