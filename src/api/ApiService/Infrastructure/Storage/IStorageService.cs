using AspireAppTemplate.ApiService.Infrastructure.Storage;
using Microsoft.AspNetCore.Http;

namespace AspireAppTemplate.ApiService.Infrastructure.Storage;

public interface IStorageService
{
    Task<Uri> UploadAsync<T>(IFormFile file, FileType supportedFileType, CancellationToken cancellationToken = default)
    where T : class;

    Task<Stream> GetFileAsync(string objectName, CancellationToken cancellationToken = default);

    void Remove(Uri? path);
}
