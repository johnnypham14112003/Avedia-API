using BusinessLogic.Extensions.Utils;
using BusinessLogic.Interfaces;
using BusinessLogic.Models.Generic;
using BusinessLogic.Models.View.Request;
using BusinessLogic.Models.View.Response;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;

namespace App.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAccountService _accountService;

    public AuthController(IAccountService accountService)
    {
        _accountService = accountService;
    }

    [AllowAnonymous]
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] AuthRq input)
    {
        var result = await _accountService.CreateAccountAsync(input);
        return StatusCode(result.StatusCode, result);
    }

    [AllowAnonymous]
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] AuthRq request)
    {
        var result = await _accountService.LoginByPasswordAsync(request);
        return StatusCode(result.StatusCode, result);
    }

    [AllowAnonymous]
    [HttpPost("refresh-token")]
    public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRq request)
    {
        var result = await _accountService.RefreshAccessToken(request);
        return StatusCode(result.StatusCode, result);
    }

    [Authorize(Roles = "Admin")]
    [HttpPatch("re-password")]
    public async Task<IActionResult> ResetPassword([FromBody] Guid targetId)
    {
        var result = await _accountService.ResetPasswordAsync(null, targetId, "12345");
        return StatusCode(result.StatusCode, result);
    }

    [Authorize]
    [HttpPost("logout")]
    public async Task<IActionResult> Logout()
    {
        var id = User.GetUserIdentity().id;
        var result = await _accountService.RevokeRefreshTokenAsync(id);
        return StatusCode(result.StatusCode, result);
    }
}
