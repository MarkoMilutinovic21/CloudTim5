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
        try
        {
            var result = await mediator.Send(command, ct);
            return Ok(result);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new { message = ex.Message });
        }
    }

    [HttpPost("activate")]
    public async Task<IActionResult> Activate([FromBody] ActivateAccountCommand command, CancellationToken ct)
    {
        await mediator.Send(command, ct);
        return Ok(new { message = "Nalog je uspešno aktiviran. Možete se prijaviti." });
    }
}
