using BusinessLogic.Extensions.Utils;
using BusinessLogic.Interfaces;
using BusinessLogic.Models.Generic;
using BusinessLogic.Models.View.Request;
using BusinessLogic.Models.View.Response;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace App.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AccountController : ControllerBase
{
    private readonly IAccountService _accountService;

    public AccountController(IAccountService accountService)
    {
        _accountService = accountService;
    }


    [Authorize]
    [HttpGet("me")]
    public async Task<IActionResult> GetMe()
    {
        var userId =User.GetUserIdentity().id;
        var result = await _accountService.GetAccountAsync(userId);
        return StatusCode(result.StatusCode, result);
    }

    [Authorize]
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await _accountService.GetAccountAsync(id);
        return StatusCode(result.StatusCode, result);
    }

    //[Authorize]
    //[HttpPatch("avatar")]
    //public async Task<IActionResult> UpdateAvatar(IFormFile file)
    //{
    //    // Kiểm tra file đính kèm
    //    if (file == null || file.Length == 0)
    //    {
    //        return BadRequest(new { success = false, message = "No file uploaded." });
    //    }

    //    var userId = GetCurrentUserId();
    //    var result = await _accountService.UpdateAvatarAsync(userId, file);
    //    return Ok(result);
    //}

    //[Authorize]
    //[HttpPatch("password")]
    //public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request)
    //{
    //    var accountId = GetCurrentUserId();
    //    var result = await _accountService.ChangePasswordAsync(accountId, request);
    //    return StatusCode(result.StatusCode, result);
    //}

    [Authorize]
    [HttpPut]
    public async Task<IActionResult> Update([FromBody] AccountRq input)
    {
        var result = await _accountService.UpdateAccountAsync(input, false);
        return StatusCode(result.StatusCode, result);
    }

    [Authorize(Roles = "Admin")]
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteAccount([FromRoute] Guid id)
    {
        var result = await _accountService.DeleteAccountAsync(id);
        return StatusCode(result.StatusCode, result);
    }

    //[Authorize(Roles = "Admin")]
    //[HttpGet]
    //public async Task<IActionResult> GetListAccount(
    //    [FromQuery] int pageNumber = 1,
    //    [FromQuery] int pageSize = 10,
    //    [FromQuery] AccountQuery? input = null)
    //{
    //    var result = await _accountService.GetAccountsListAsync(pageNumber, pageSize, input);
    //    return StatusCode(result.StatusCode, result);
    //}

    //[Authorize(Roles = "Admin")]
    //[HttpPatch("role")]
    //public async Task<IActionResult> ChangeRole([FromBody] AccountRole newAccount)
    //{
    //    var result = await _accountService.ChangeRoleAsync(newAccount.Id, newAccount.Role);
    //    return StatusCode(result.StatusCode, result);
    //}
}
