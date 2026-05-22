using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Voxera.Application.Commands.Auth;
using Voxera.Application.Interfaces;

namespace Voxera.API.Controllers;

[ApiController]
[Route("api/v1/auth")]
[Produces("application/json")]
public class AuthController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ITokenService _tokenService;

    public AuthController(IMediator mediator, ITokenService tokenService)
    {
        _mediator = mediator;
        _tokenService = tokenService;
    }

    /// <summary>Register a new company and admin user</summary>
    [HttpPost("register")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request, CancellationToken ct)
    {
        var result = await _mediator.Send(new RegisterCommand(
            request.CompanyName, request.FirstName, request.LastName, request.Email, request.Password, request.Phone), ct);
        return Ok(result);
    }

    /// <summary>Login with email and password</summary>
    [HttpPost("login")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login([FromBody] LoginRequest request, CancellationToken ct)
    {
        var ip = HttpContext.Connection.RemoteIpAddress?.ToString();
        var userAgent = Request.Headers.UserAgent.ToString();
        var result = await _mediator.Send(new LoginCommand(request.Email, request.Password, ip, userAgent), ct);
        return Ok(result);
    }

    /// <summary>Refresh access token using refresh token</summary>
    [HttpPost("refresh")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Refresh([FromBody] RefreshRequest request, CancellationToken ct)
    {
        var (accessToken, refreshToken) = await _tokenService.RefreshTokensAsync(request.RefreshToken, ct);
        return Ok(new { access_token = accessToken, refresh_token = refreshToken });
    }

    /// <summary>Get current user info</summary>
    [HttpGet("me")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IActionResult Me()
    {
        var userId = User.FindFirst("sub")?.Value;
        var email = User.FindFirst("email")?.Value;
        var role = User.FindFirst("role")?.Value;
        var companyId = User.FindFirst("company_id")?.Value;
        var fullName = User.FindFirst("full_name")?.Value;
        return Ok(new { userId, email, role, companyId, fullName });
    }

    /// <summary>Logout (invalidate refresh token)</summary>
    [HttpPost("logout")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public IActionResult Logout()
    {
        // In production: invalidate refresh token in DB
        return NoContent();
    }
}

public record RegisterRequest(string CompanyName, string FirstName, string LastName, string Email, string Password, string? Phone = null);
public record LoginRequest(string Email, string Password);
public record RefreshRequest(string RefreshToken);
