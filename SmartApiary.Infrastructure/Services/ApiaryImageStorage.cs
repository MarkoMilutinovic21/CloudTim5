namespace SmartApiary.Infrastructure.Services;

using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.Extensions.Options;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Processing;
using SmartApiary.Application.Common.Interfaces;

public class ApiaryImageStorage(IOptions<AzureBlobOptions> options) : IApiaryImageStorage
{
    public async Task<ApiaryImageUploadResult> UploadAsync(
        Stream imageStream,
        string fileName,
        string contentType,
        CancellationToken ct = default)
    {
        BlobContainerClient container = new(
            options.Value.ConnectionString,
            options.Value.ApiaryImagesContainer);

        await container.CreateIfNotExistsAsync(PublicAccessType.Blob, cancellationToken: ct);

        string extension = Path.GetExtension(fileName);
        string baseName = $"{Guid.NewGuid():N}";
        string originalBlobName = $"original/{baseName}{extension}";
        string thumbnailBlobName = $"thumbnails/{baseName}.jpg";

        await using MemoryStream originalBuffer = new();
        await imageStream.CopyToAsync(originalBuffer, ct);
        originalBuffer.Position = 0;

        BlobClient originalBlob = container.GetBlobClient(originalBlobName);
        await originalBlob.UploadAsync(
            originalBuffer,
            new BlobHttpHeaders { ContentType = contentType },
            cancellationToken: ct);

        originalBuffer.Position = 0;
        using Image image = await Image.LoadAsync(originalBuffer, ct);
        image.Mutate(context => context.Resize(new ResizeOptions
        {
            Mode = ResizeMode.Max,
            Size = new Size(320, 240)
        }));

        await using MemoryStream thumbnailBuffer = new();
        await image.SaveAsJpegAsync(thumbnailBuffer, new JpegEncoder { Quality = 80 }, ct);
        thumbnailBuffer.Position = 0;

        BlobClient thumbnailBlob = container.GetBlobClient(thumbnailBlobName);
        await thumbnailBlob.UploadAsync(
            thumbnailBuffer,
            new BlobHttpHeaders { ContentType = "image/jpeg" },
            cancellationToken: ct);

        return new ApiaryImageUploadResult(
            originalBlob.Uri.ToString(),
            thumbnailBlob.Uri.ToString());
    }
}
