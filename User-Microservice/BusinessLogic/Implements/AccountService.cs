using BusinessLogic.Extensions.Exceptions;
using BusinessLogic.Extensions.Utils;
using BusinessLogic.Interfaces;
using BusinessLogic.Models.Generic;
using BusinessLogic.Models.View.Request;
using BusinessLogic.Models.View.Request.Query;
using BusinessLogic.Models.View.Response;
using DataAccess.Interfaces;
using DataAccess.Models;
using LinqKit;
using Mapster;

namespace BusinessLogic.Implements;

public class AccountService : IAccountService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITokenService _tokenService;

    public AccountService(IUnitOfWork unitOfWork, ITokenService tokenService)
    {
        _unitOfWork = unitOfWork;
        _tokenService = tokenService;
    }

    // ===========================< METHODS >===========================
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

    // ----------------------------< CRUD >----------------------------
    public async Task<ApiResult<bool>> CreateAccountAsync(AuthRq request)
    {
        var accountRepo = _unitOfWork.GetRepository<Account>();

        // 1. Validate exist mail
        bool isEmailExist = await accountRepo.AnyAsync(a => a.Email.ToLower() == request.Email.ToLower());
        if (isEmailExist)
            throw new ConflictException("This email already been used!");

        // 2. Add default to Database
        await accountRepo.AddAsync(new Account
        {
            UserName = StringUtils.GetUsername(request.Email),
            Email = request.Email,
            PasswordHash = StringUtils.HashPassword(request.Password),
        });
        await _unitOfWork.CompleteAsync();

        return ApiResult<bool>.Created(true);
    }

    public async Task<ApiResult<AccountRs>> GetAccountByIdAsync(Guid id)
    {
        var account = await _unitOfWork.GetRepository<Account>().GetByIdAsync(id)
            ?? throw new NotFoundException("Not found this account match id!");

        return ApiResult<AccountRs>.Ok(account.Adapt<AccountRs>());
    }

    public async Task<ApiResult<IEnumerable<AccountRs>>> GetAccountsAsync(PagingQueryRq<AccountQr> input)
    {
        // Query form builder
        var predicate = PredicateBuilder.New<Account>(true);

        // ------------------------------------------
        if (!string.IsNullOrWhiteSpace(input.Keyword))
        {
            predicate = predicate.And(q => q.UserName.Contains(input.Keyword, StringComparison.OrdinalIgnoreCase));
            predicate = predicate.And(q => q.UserName.Contains(input.Keyword, StringComparison.OrdinalIgnoreCase));
        }

        var accounts = await _unitOfWork.GetRepository<Account>().GetPagedAsync(
            predicate: predicate,
            pageNumber: input.PageNumber,
            pageSize: input.PageSize,
            orderBy: q => q.OrderByDescending(a => a.UserName)
        );

        return ApiResult<IEnumerable<AccountRs>>.Ok(accounts.Adapt<IEnumerable<AccountRs>>());
    }

    public async Task<ApiResult<AccountRs>> UpdateAccountAsync(Guid id, AccountRq request)
    {
        var accountRepo = _unitOfWork.GetRepository<Account>();

        // 1. Lấy thông tin tài khoản (có tracking để update)
        var existAccount = await accountRepo.GetOneAsync(a => a.Id == id, hasTracking: true);

        if (existAccount == null)
            throw new BadRequestException("Không tìm thấy tài khoản để cập nhật!");

        // 2. Cập nhật các trường được phép thay đổi
        // Có thể dùng Mapster để map trực tiếp: request.Adapt(existAccount);
        // Hoặc gán tay để kiểm soát chặt chẽ:
        existAccount.UserName = request.UserName ?? existAccount.UserName;
        existAccount.AvatarUrl = request.AvatarUrl ?? existAccount.AvatarUrl;
        existAccount.Gender = request.Gender ?? existAccount.Gender;
        existAccount.Nationality = request.Nationality ?? existAccount.Nationality;

        await accountRepo.UpdateAsync(existAccount);
        await _unitOfWork.CompleteAsync();

        return ApiResult<AccountRs>.Ok(existAccount.Adapt<AccountRs>());
    }

    public async Task<ApiResult<bool>> DeleteAccountAsync(Guid id)
    {
        var accountRepo = _unitOfWork.GetRepository<Account>();

        var existAccount = await accountRepo.GetByIdAsync(id);
        if (existAccount == null)
            throw new BadRequestException("Không tìm thấy tài khoản cần xóa!");

        // Soft delete
        existAccount.Status = "Deleted";
        await accountRepo.UpdateAsync(existAccount);

        await _unitOfWork.CompleteAsync();

        return ApiResult<bool>.Ok(true);
    }
}
