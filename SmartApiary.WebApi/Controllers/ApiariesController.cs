namespace SmartApiary.WebApi.Controllers;

using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartApiary.Application.Common.Interfaces;
using SmartApiary.Application.Features.Apiaries.Commands;
using SmartApiary.Application.Features.Apiaries.Queries;
using System.Security.Claims;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Beekeeper")]
public class ApiariesController(
    IMediator mediator,
    IApiaryImageStorage imageStorage,
    IApiaryRepository apiaryRepository) : ControllerBase
{
    private Guid GetUserId() =>
        Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    [HttpGet]
    public async Task<IActionResult> GetApiaries(CancellationToken ct)
    {
        var apiaries = await mediator.Send(new GetApiaresQuery(GetUserId()), ct);
        return Ok(apiaries);
    }

    [HttpPost]
    public async Task<IActionResult> CreateApiary([FromForm] ApiaryFormRequest request, CancellationToken ct)
    {
        var image = await UploadImageAsync(request.Image, ct);

        await mediator.Send(new CreateApiaryCommand(
            request.Name,
            request.Description ?? string.Empty,
            request.Location,
            request.Latitude,
            request.Longitude,
            image.ImageUrl,
            image.ThumbnailUrl,
            GetUserId()), ct);

        return Ok(new { message = "Pčelinjak uspešno kreiran." });
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateApiary(Guid id, [FromForm] ApiaryFormRequest request, CancellationToken ct)
    {
        var existing = await apiaryRepository.GetByIdAsync(id, ct);

        if (existing is null)
            return NotFound(new { message = "Pčelinjak nije pronađen." });

        if (existing.OwnerId != GetUserId())
            return Forbid();

        var image = request.Image is null
            ? new ApiaryImageUploadResult(existing.ImageUrl, existing.ThumbnailUrl)
            : await UploadImageAsync(request.Image, ct);

        await mediator.Send(new UpdateApiaryCommand(
            id,
            request.Name,
            request.Description ?? string.Empty,
            request.Location,
            request.Latitude,
            request.Longitude,
            image.ImageUrl,
            image.ThumbnailUrl,
            GetUserId()), ct);

        return Ok(new { message = "Pčelinjak uspešno izmenjen." });
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteApiary(Guid id, CancellationToken ct)
    {
        await mediator.Send(new DeleteApiaryCommand(id, GetUserId()), ct);
        return Ok(new { message = "Pčelinjak uspešno obrisan." });
    }

    private async Task<ApiaryImageUploadResult> UploadImageAsync(IFormFile? image, CancellationToken ct)
    {
        if (image is null || image.Length == 0)
            return new ApiaryImageUploadResult(string.Empty, string.Empty);

        if (!image.ContentType.StartsWith("image/", StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException("Fajl mora biti slika.");

        await using Stream stream = image.OpenReadStream();
        return await imageStorage.UploadAsync(stream, image.FileName, image.ContentType, ct);
    }
}

public class ApiaryFormRequest
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Location { get; set; } = string.Empty;
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public IFormFile? Image { get; set; }
}
