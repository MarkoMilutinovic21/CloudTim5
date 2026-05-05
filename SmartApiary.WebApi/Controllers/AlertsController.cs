namespace SmartApiary.WebApi.Controllers;

using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartApiary.Application.Features.Alerts.Queries;
using System.Security.Claims;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Beekeeper")]
public class AlertsController(IMediator mediator) : ControllerBase
{
    private Guid GetUserId() =>
        Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    [HttpGet]
    public async Task<IActionResult> GetAlerts(CancellationToken ct)
    {
        IReadOnlyCollection<BeekeeperAlertDto> alerts =
            await mediator.Send(new GetBeekeeperAlertsQuery(GetUserId()), ct);

        return Ok(alerts);
    }
}
