using BusinessLogic.Interfaces;
using BusinessLogic.Models.Generic;
using BusinessLogic.Models.View.Request;
using BusinessLogic.Models.View.Response;
using Microsoft.AspNetCore.Authorization;
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
    [HttpPost("login")]
    public async Task<ActionResult<ApiResult<AuthRs>>> Login([FromBody] AuthRq request)
    {
        var result = await _accountService.LoginByPasswordAsync(request);
        return StatusCode(result.StatusCode, result);
    }

    [AllowAnonymous]
    [HttpPost("refresh-token")]
    public async Task<ActionResult<ApiResult<AuthRs>>> RefreshToken([FromBody] RefreshTokenRq request)
    {
        var result = await _accountService.RefreshAccessToken(request);
        return StatusCode(result.StatusCode, result);
    }
}
