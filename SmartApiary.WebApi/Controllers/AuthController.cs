namespace SmartApiary.WebApi.Controllers;

using MediatR;
using Microsoft.AspNetCore.Mvc;
using SmartApiary.Application.Features.Auth.Commands;

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
}