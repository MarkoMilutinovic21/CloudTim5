namespace SmartApiary.WebApi.Controllers;

using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartApiary.Application.Features.HiveJournal.Commands;
using SmartApiary.Application.Features.HiveJournal.Queries;
using System.Security.Claims;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Beekeeper")]
public class HiveJournalController(IMediator mediator) : ControllerBase
{
    private Guid GetUserId() =>
        Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    [HttpGet("options")]
    public async Task<IActionResult> GetJournalHiveOptions(CancellationToken ct)
    {
        var options = await mediator.Send(new GetJournalHiveOptionsQuery(GetUserId()), ct);
        return Ok(options);
    }

    [HttpGet("{hiveId:guid}")]
    public async Task<IActionResult> GetJournalEntries(Guid hiveId, CancellationToken ct)
    {
        var entries = await mediator.Send(new GetJournalEntriesQuery(hiveId, GetUserId()), ct);
        return Ok(entries);
    }

    [HttpPost]
    public async Task<IActionResult> CreateJournalEntry([FromBody] CreateJournalEntryCommand command, CancellationToken ct)
    {
        Guid entryId = await mediator.Send(command with { BeekeeperId = GetUserId() }, ct);
        return Ok(new { id = entryId, message = "Zapis u dnevniku uspešno kreiran." });
    }

    [HttpPut("{hiveId:guid}/{entryId:guid}")]
    public async Task<IActionResult> UpdateJournalEntry(
        Guid hiveId,
        Guid entryId,
        [FromBody] UpdateJournalEntryCommand command,
        CancellationToken ct)
    {
        var commandWithIds = command with { EntryId = entryId, HiveId = hiveId, BeekeeperId = GetUserId() };
        await mediator.Send(commandWithIds, ct);
        return Ok(new { message = "Zapis u dnevniku uspešno izmenjen." });
    }

    [HttpDelete("{hiveId}/{entryId}")]
    public async Task<IActionResult> DeleteJournalEntry(Guid hiveId, Guid entryId, CancellationToken ct)
    {
        await mediator.Send(new DeleteJournalEntryCommand(hiveId, entryId, GetUserId()), ct);
        return Ok(new { message = "Zapis u dnevniku uspešno obrisan." });
    }
}
