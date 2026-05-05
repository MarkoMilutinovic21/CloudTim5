namespace SmartApiary.WebApi.Controllers;

using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartApiary.Application.Features.HiveJournal.Commands;
using SmartApiary.Application.Features.HiveJournal.Queries;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Beekeeper")]
public class HiveJournalController(IMediator mediator) : ControllerBase
{
    [HttpGet("options")]
    public async Task<IActionResult> GetJournalHiveOptions(CancellationToken ct)
    {
        var options = await mediator.Send(new GetJournalHiveOptionsQuery(), ct);
        return Ok(options);
    }

    [HttpGet("{hiveId:guid}")]
    public async Task<IActionResult> GetJournalEntries(Guid hiveId, CancellationToken ct)
    {
        var entries = await mediator.Send(new GetJournalEntriesQuery(hiveId), ct);
        return Ok(entries);
    }

    [HttpPost]
    public async Task<IActionResult> CreateJournalEntry([FromBody] CreateJournalEntryCommand command, CancellationToken ct)
    {
        Guid entryId = await mediator.Send(command, ct);
        return Ok(new { id = entryId, message = "Zapis u dnevniku uspešno kreiran." });
    }

    [HttpDelete("{hiveId}/{entryId}")]
    public async Task<IActionResult> DeleteJournalEntry(Guid hiveId, Guid entryId, CancellationToken ct)
    {
        await mediator.Send(new DeleteJournalEntryCommand(hiveId, entryId), ct);
        return Ok(new { message = "Zapis u dnevniku uspešno obrisan." });
    }
}
