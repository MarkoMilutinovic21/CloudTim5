namespace SmartApiary.WebApi.Controllers;

using MediatR;
using Microsoft.AspNetCore.Mvc;
using SmartApiary.Application.Features.Auth.Commands;
using SmartApiary.Application.Features.Users.Commands;

[ApiController]
[Route("api/[controller]")]
public class AuthController(IMediator mediator) : ControllerBase
{
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginCommand command, CancellationToken ct)
    {
        var result = await mediator.Send(command, ct);
        return Ok(result);
    }

    [HttpPost("activate")]
    public async Task<IActionResult> Activate([FromBody] ActivateAccountCommand command, CancellationToken ct)
    {
        await mediator.Send(command, ct);
        return Ok(new { message = "Nalog je uspešno aktiviran. Možete se prijaviti." });
    }
}