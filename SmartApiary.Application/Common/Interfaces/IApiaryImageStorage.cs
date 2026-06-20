namespace SmartApiary.Application.Common.Interfaces;

public interface IApiaryImageStorage
{
    Task<ApiaryImageUploadResult> UploadAsync(
        Stream imageStream,
        string fileName,
        string contentType,
        CancellationToken ct = default);
}

public record ApiaryImageUploadResult(string ImageUrl, string ThumbnailUrl);
