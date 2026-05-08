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
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.EntityFrameworkCore;

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

    public async Task<ApiResult<bool>> RevokeRefreshTokenAsync(Guid accountId)
    {
        var account = await _unitOfWork.GetRepository<Account>().GetByIdAsync(accountId);
        if (account != null)
        {
            account.RefreshToken = null;
            account.RefreshTokenExpirytime = null;
        }
        return (await _unitOfWork.CompleteAsync() > 0)
            ? ApiResult<bool>.Created(true) : ApiResult<bool>.Failure(false);
    }

    public async Task<ApiResult<bool>> ResetPasswordAsync(Account? alreadyQueried, Guid accountId, string newPassword)
    {
        var hashedPassword = StringUtils.HashPassword(newPassword);
        if (alreadyQueried is null)
        {
            var account = await _unitOfWork.GetRepository<Account>().GetByIdAsync(accountId)
                ?? throw new NotFoundException("Account not found!");

            // Update for new queried account
            account.PasswordHash = hashedPassword;
            account.RefreshToken = null;
            account.RefreshTokenExpirytime = null;
        }
        else
        {
            // Update for queried account from params
            alreadyQueried.PasswordHash = hashedPassword;
            alreadyQueried.RefreshToken = null;
            alreadyQueried.RefreshTokenExpirytime = null;
        }

        return (await _unitOfWork.CompleteAsync() > 0)
            ? ApiResult<bool>.Created(true) : ApiResult<bool>.Failure(false);
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

        return (await _unitOfWork.CompleteAsync() > 0)
            ? ApiResult<bool>.Created(true) : ApiResult<bool>.Failure(false);
    }

    public async Task<ApiResult<AccountRs>> GetAccountAsync(Guid id, bool includeBadge = false)
    {
        var account = (includeBadge == false)
            ? await _unitOfWork.GetRepository<Account>().GetByIdAsync(id)
            : await _unitOfWork.GetRepository<Account>().GetOneAsync(
                a => a.Id == id,
                q => q.Include(a => a.AccountBadges).ThenInclude(ab => ab.Badge),
                hasTracking: false
            );

        return (account is null)
            ? throw new NotFoundException("Not found this account match id!")
            : ApiResult<AccountRs>.Ok(account.Adapt<AccountRs>());
    }

    public async Task<ApiResult<PagedResult<AccountRs>>> GetAccountsPageAsync(PagingQueryRq<AccountQr> input)
    {
        // Query form builder
        var predicate = PredicateBuilder.New<Account>(true);

        // ------------------------------------------
        if (!string.IsNullOrWhiteSpace(input.Keyword))
        {
            predicate = predicate.And(a => a.UserName.Contains(input.Keyword));
            predicate = predicate.And(a => a.Email.Contains(input.Keyword));
        }

        if (input.AdvanceInput is not null)
        {
            // IsVerified
            if (input.AdvanceInput.IsVerified.HasValue)
                predicate = predicate.And(a => a.IsVerified == input.AdvanceInput.IsVerified);

            // Gender
            switch (input.AdvanceInput.Gender)
            {
                case 0:
                    predicate = predicate.And(a => a.Gender == false);
                    break;
                case 1:
                    predicate = predicate.And(a => a.Gender == true);
                    break;
                case 2:
                    predicate = predicate.And(a => a.Gender == null);
                    break;
            }

            // Nationality
            if (!string.IsNullOrWhiteSpace(input.AdvanceInput.Nationality))
                predicate = predicate.And(a => a.Nationality!.Equals(input.AdvanceInput.Nationality));

            // JoinedDate
            if (input.AdvanceInput.FromDate.HasValue)
                predicate = predicate.And(a => a.JoinedDate >= input.AdvanceInput.FromDate);
            if (input.AdvanceInput.ToDate.HasValue)
                predicate = predicate.And(a => a.JoinedDate < input.AdvanceInput.ToDate.Value.AddDays(1));

            // Role
            if (!string.IsNullOrWhiteSpace(input.AdvanceInput.Role))
                predicate = predicate.And(a => a.Role.Equals(input.AdvanceInput.Role));

            // Status
            if (!string.IsNullOrWhiteSpace(input.AdvanceInput.Status))
                predicate = predicate.And(a => a.Status.Equals(input.AdvanceInput.Status));
        }
        // ------------------------------------------
        var accountRepo = _unitOfWork.GetRepository<Account>();
        var accounts = (
            await accountRepo.GetPagedAsync(
                predicate: predicate,
                pageNumber: input.PageNumber,
                pageSize: input.PageSize)
            ).Adapt<IEnumerable<AccountRs>>();

        return ApiResult<PagedResult<AccountRs>>.Ok(
            new PagedResult<AccountRs>
            {
                TotalCount = await accountRepo.CountAsync(x => true),
                PageSize = input.PageSize,
                PageIndex = input.PageNumber,
                DataList = accounts
            });
    }

    public async Task<ApiResult<bool>> UpdateAccountAsync(AccountRq request, bool updateAll = true)
    {
        var accountRepo = _unitOfWork.GetRepository<Account>();

        var existAccount = await accountRepo.GetOneAsync(a => a.Id == request.Id, hasTracking: true)
            ?? throw new NotFoundException("Not found this Id account!");

        var temp = existAccount;

        // Update All
        request.Adapt(existAccount);

        // If not update all, keep field that user can't edit
        if (updateAll == false)
        {
            existAccount.IsVerified = temp.IsVerified;
            existAccount.JoinedDate = temp.JoinedDate;
            existAccount.MeritPoint = temp.MeritPoint;
            existAccount.Role = temp.Role;
        }

        return (await _unitOfWork.CompleteAsync() > 0)
            ? ApiResult<bool>.Ok(true) : ApiResult<bool>.Failure(false);
    }

    public async Task<ApiResult<bool>> DeleteAccountAsync(Guid id)
    {
        var accountRepo = _unitOfWork.GetRepository<Account>();

        var existAccount = await accountRepo.GetByIdAsync(id)
            ?? throw new NotFoundException("Not found this Id account!");

        // Soft delete
        existAccount.Status = "Deleted";

        return (await _unitOfWork.CompleteAsync() > 0)
            ? ApiResult<bool>.Created(true) : ApiResult<bool>.Failure(false);
    }
}
