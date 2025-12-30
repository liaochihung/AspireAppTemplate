using System.Reflection;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Minio;
using Minio.DataModel.Args;

namespace AspireAppTemplate.ApiService.Infrastructure.Storage;

public class MinioStorageService(IConfiguration configuration) : IStorageService
{
    private readonly IMinioClient _minioClient = new MinioClient()
        .WithEndpoint(new Uri(configuration["MinIO:Endpoint"] ?? "http://minio:9000").Authority)
        .WithCredentials(configuration["MinIO:AccessKey"], configuration["MinIO:SecretKey"])
        .WithSSL(false)
        .Build();

    private const string BucketName = "files";

    public async Task<Uri> UploadAsync<T>(IFormFile file, FileType supportedFileType, CancellationToken cancellationToken = default)
        where T : class
    {
        var extension = Path.GetExtension(file.FileName).ToLower();
        // Simple validation found in FileType description (optional, can be improved)
        // if (!supportedFileType.GetDescriptionList().Contains(extension)) throw ...

        string folder = typeof(T).Name.ToLower();
        string fileName = $"{Guid.NewGuid()}{extension}";
        string objectName = $"{folder}/{fileName}";

        await EnsureBucketExists(cancellationToken);

        using var stream = file.OpenReadStream();
        var putObjectArgs = new PutObjectArgs()
            .WithBucket(BucketName)
            .WithObject(objectName)
            .WithStreamData(stream)
            .WithObjectSize(stream.Length)
            .WithContentType(file.ContentType);

        await _minioClient.PutObjectAsync(putObjectArgs, cancellationToken);
        
        // Setup for public access or return internal URI
        // For now, we return a relative URI which the API can use or frontend can use if we proxy or if we use public endpoint.
        // But to follow the prompt's request for "direct open", we might want a Presigned URL or public URL.
        // Let's return the internal URI for reference: "s3://files/..." or just the path?
        // StarterKit returns full HTTP URI.
        // We will return a relative path for now, and let the caller construct the full URL or we use a helper.
        // Actually, to make it usable immediately, let's return the object name as URI.
        return new Uri(objectName, UriKind.Relative);
    }

    public async Task<Stream> GetFileAsync(string objectName, CancellationToken cancellationToken = default)
    {
        var memoryStream = new MemoryStream();
        var args = new GetObjectArgs()
            .WithBucket(BucketName)
            .WithObject(objectName)
            .WithCallbackStream((stream) =>
            {
                stream.CopyTo(memoryStream);
            });
        
        await _minioClient.GetObjectAsync(args, cancellationToken);
        
        memoryStream.Position = 0;
        return memoryStream;
    }

    public async Task<List<StorageFile>> ListFilesAsync(CancellationToken cancellationToken = default)
    {
        var files = new List<StorageFile>();
        
        var args = new ListObjectsArgs()
            .WithBucket(BucketName)
            .WithRecursive(true);

        var list = _minioClient.ListObjectsEnumAsync(args, cancellationToken);
        
        await foreach (var item in list)
        {
            files.Add(new StorageFile(item.Key, (long)item.Size));
        }

        return files;
    }

    public async Task RemoveAsync(string objectName, CancellationToken cancellationToken = default)
    {
        try 
        {
            var args = new RemoveObjectArgs()
                .WithBucket(BucketName)
                .WithObject(objectName);
            await _minioClient.RemoveObjectAsync(args, cancellationToken);
        }
        catch 
        {
            // Log error
        }
    }

    private async Task EnsureBucketExists(CancellationToken cancellationToken)
    {
        var beArgs = new BucketExistsArgs().WithBucket(BucketName);
        bool found = await _minioClient.BucketExistsAsync(beArgs, cancellationToken);
        if (!found)
        {
            var mbArgs = new MakeBucketArgs().WithBucket(BucketName);
            await _minioClient.MakeBucketAsync(mbArgs, cancellationToken);
            
            // Set Policy to Public Read for 'files' bucket
            // This allows direct browser access via http://localhost:9000/files/object
            var policy = $@"{{
                ""Version"": ""2012-10-17"",
                ""Statement"": [
                    {{
                        ""Effect"": ""Allow"",
                        ""Principal"": {{""AWS"": ""*""}},
                        ""Action"": [""s3:GetObject""],
                        ""Resource"": [""arn:aws:s3:::{BucketName}/*""]
                    }}
                ]
            }}";
            var setPolicyArgs = new SetPolicyArgs().WithBucket(BucketName).WithPolicy(policy);
            await _minioClient.SetPolicyAsync(setPolicyArgs, cancellationToken);
        }
    }
}
