namespace SmartApiary.WebApi.Controllers;

using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using SmartApiary.Application.Common.Interfaces;
using SmartApiary.Application.Features.SprayingRecords.Queries;
using SmartApiary.Domain.Models;
using System.Security.Claims;

[ApiController]
[Route("api/farmer/spraying-records")]
[Authorize(Roles = "Farmer")]
public class FarmerSprayingRecordsController(IMediator mediator, IParcelRepository parcelRepository) : ControllerBase
{
    private Guid GetUserId() =>
        Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    [HttpGet("{parcelId}")]
    public async Task<IActionResult> Get(
        Guid parcelId,
        [FromQuery] DateTime? from,
        [FromQuery] DateTime? to,
        CancellationToken ct)
    {
        var records = await mediator.Send(new GetSprayingRecordsQuery(parcelId, GetUserId(), from, to), ct);
        return Ok(records);
    }

    [HttpGet("{parcelId}/export-pdf")]
    public async Task<IActionResult> ExportPdf(
        Guid parcelId,
        [FromQuery] DateTime? from,
        [FromQuery] DateTime? to,
        CancellationToken ct)
    {
        var records = await mediator.Send(new GetSprayingRecordsQuery(parcelId, GetUserId(), from, to), ct);
        var parcel = await parcelRepository.GetByIdAsync(parcelId, ct);
        var parcelName = parcel?.Name ?? parcelId.ToString();

        var pdfBytes = GeneratePdf(parcelName, records, from, to);
        return File(pdfBytes, "application/pdf", $"karton-prskanja-{DateTime.Now:yyyyMMdd}.pdf");
    }

    private static byte[] GeneratePdf(
        string parcelName,
        IReadOnlyCollection<SprayingRecord> records,
        DateTime? from,
        DateTime? to)
    {
        return QuestPDF.Fluent.Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(2, QuestPDF.Infrastructure.Unit.Centimetre);
                page.DefaultTextStyle(x => x.FontSize(11));

                page.Header().Column(col =>
                {
                    col.Item().Text("Digitalni karton prskanja").FontSize(18).Bold();
                    col.Item().Text($"Parcela: {parcelName}").FontSize(12);
                    col.Item().Text(
                        $"Period: {from?.ToString("dd.MM.yyyy") ?? "—"} — {to?.ToString("dd.MM.yyyy") ?? "—"}"
                    ).FontSize(10).FontColor(Colors.Grey.Medium);
                    col.Item().Text($"Generisano: {DateTime.Now:dd.MM.yyyy HH:mm}").FontSize(10).FontColor(Colors.Grey.Medium);
                    col.Item().PaddingTop(8).LineHorizontal(1).LineColor(Colors.Grey.Lighten1);
                });

                page.Content().PaddingTop(12).Table(table =>
                {
                    table.ColumnsDefinition(columns =>
                    {
                        columns.RelativeColumn(3);
                        columns.RelativeColumn(3);
                        columns.RelativeColumn(2);
                        columns.RelativeColumn(3);
                        columns.RelativeColumn(3);
                        columns.RelativeColumn(4);
                    });

                    table.Header(header =>
                    {
                        header.Cell().Background(Colors.Grey.Lighten3).Padding(6).Text("Datum i vreme").Bold();
                        header.Cell().Background(Colors.Grey.Lighten3).Padding(6).Text("Završetak").Bold();
                        header.Cell().Background(Colors.Grey.Lighten3).Padding(6).Text("Trajanje").Bold();
                        header.Cell().Background(Colors.Grey.Lighten3).Padding(6).Text("Preparat").Bold();
                        header.Cell().Background(Colors.Grey.Lighten3).Padding(6).Text("Kultura").Bold();
                        header.Cell().Background(Colors.Grey.Lighten3).Padding(6).Text("Vreme").Bold();
                    });

                    foreach (var record in records)
                    {
                        table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten3).Padding(6)
                            .Text(record.StartTime.ToString("dd.MM.yyyy HH:mm"));
                        table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten3).Padding(6)
                            .Text(record.EndTime.ToString("dd.MM.yyyy HH:mm"));
                        table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten3).Padding(6)
                            .Text($"{record.DurationHours} h");
                        table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten3).Padding(6)
                            .Text(record.ChemicalName);
                        table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten3).Padding(6)
                            .Text(string.IsNullOrWhiteSpace(record.CropName) ? "—" : record.CropName);
                        table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten3).Padding(6)
                            .Text(FormatWeather(record));
                    }
                });

                page.Footer().AlignCenter().Text(text =>
                {
                    text.Span("Strana ");
                    text.CurrentPageNumber();
                    text.Span(" od ");
                    text.TotalPages();
                });
            });
        }).GeneratePdf();
    }

    private static string FormatWeather(SprayingRecord record)
    {
        List<string> values = new();
        if (!string.IsNullOrWhiteSpace(record.WeatherDescription))
            values.Add(record.WeatherDescription);
        if (record.WindSpeedMs.HasValue)
            values.Add($"vetar {record.WindSpeedMs.Value:0.0} m/s");
        if (record.HadPrecipitation)
            values.Add("padavine");
        return values.Count == 0 ? "Nema podataka" : string.Join(", ", values);
    }
}
