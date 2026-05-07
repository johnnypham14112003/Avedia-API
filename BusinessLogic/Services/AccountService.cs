using BusinessLogic.Extensions.Exceptions;
using BusinessLogic.Extensions.Utils;
using BusinessLogic.Interfaces;
using BusinessLogic.Models.Generic;
using BusinessLogic.Models.View.Request;
using BusinessLogic.Models.View.Response;
using DataAccess.Interfaces;
using DataAccess.Models;
using Mapster;

namespace BusinessLogic.Services;

public class AccountService : IAccountService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITokenService _tokenService;

    public AccountService(IUnitOfWork unitOfWork, ITokenService tokenService)
    {
        _unitOfWork = unitOfWork;
        _tokenService = tokenService;
    }

    public async Task<ApiResult<AuthRs>> LoginByPasswordAsync(AuthRq authRequest)
    {
        // Validate email
        if (BoolUtils.IsValidEmail(authRequest.Email) == false)
            throw new BadRequestException("Invalid Email Format!");

        // Check account in database
        var existAccount = await _unitOfWork.GetRepository<Account>().GetOneAsync(
            acc => acc.Email.ToLower().Equals(authRequest.Email.ToLower()));

        // Validate password with hashed password using custom method for security
        if (existAccount == null ||
            BoolUtils.VerifyPassword(authRequest.Password, existAccount.PasswordHash) == false)
            throw new BadRequestException("Invalid email or password!");

        // Generate tokens
        var accessToken = _tokenService.GenerateAccessToken(existAccount);
        var refreshToken = _tokenService.GenerateRefreshToken();
        var refreshExpireTime = _tokenService.GetExpirationTimes();

        // Save refresh token infos
        existAccount.RefreshToken = refreshToken;
        existAccount.RefreshTokenExpirytime = refreshExpireTime;
        await _unitOfWork.CompleteAsync();

        return ApiResult<AuthRs>.Ok(new AuthRs
        {
            Account = existAccount.Adapt<AccountRs>(),
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            RefreshExpireTime = refreshExpireTime
        });
    }

    public async Task<ApiResult<AuthRs>> RefreshAccessToken(RefreshTokenRq request)
    {
        var principal = await _tokenService.GetPrincipalFromExpiredTokenAsync(request.AccessToken)
            ?? throw new BadRequestException("Token invalid.");

        // Extract Id from Tokens
        var userIdString = principal.Claims.FirstOrDefault(c => c.Type == "Id")?.Value;
        if (!Guid.TryParse(userIdString, out Guid userId) || userId == Guid.Empty)
            throw new UnauthorizedException("Token don't have valid User Id.");

        // Query account base on id from token
        var account = await _unitOfWork.GetRepository<Account>().GetOneAsync(u => u.Id == userId);

        // Validate refresh token
        if (account == null ||
            account.RefreshToken != request.RefreshToken ||
            account.RefreshTokenExpirytime <= DateTime.Now)
        {
            throw new BadRequestException("Refresh Token is expired or invalid. Please login again.");
        }

        // Generate new tokens (new refresh token for Rotation - more security if refresh token is leaked)
        var newAccessToken = _tokenService.GenerateAccessToken(account);
        var newRefreshToken = _tokenService.GenerateRefreshToken();

        // Keep create new expire time every time AccessToken need refresh
        // var newRefreshExpireTime = _tokenService.GetExpirationTimes();

        // After certain days -> must login to get new expire
        var refreshExpireTime = account.RefreshTokenExpirytime ?? _tokenService.GetExpirationTimes();

        // Save refresh token infos
        account.RefreshToken = newRefreshToken;
        account.RefreshTokenExpirytime = refreshExpireTime;
        await _unitOfWork.CompleteAsync();

        return ApiResult<AuthRs>.Ok(new AuthRs
        {
            AccessToken = newAccessToken,
            RefreshToken = newRefreshToken,
            RefreshExpireTime = refreshExpireTime
        });
    }
}
