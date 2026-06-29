using BusinessLogic.DTOs.Generic;
using BusinessLogic.DTOs.Messages;
using BusinessLogic.DTOs.Messages.Request;
using BusinessLogic.DTOs.Messages.Request.Query;
using BusinessLogic.DTOs.Messages.Response;
using BusinessLogic.Extensions.Utils;
using BusinessLogic.Interfaces;
using DataAccess.Interfaces;
using DataAccess.Models;
using LinqKit;
using Mapster;
using Microsoft.EntityFrameworkCore;

namespace BusinessLogic.Implements;

public class AccountService(IUnitOfWork unitOfWork) : IAccountService
{
    private readonly IUnitOfWork _unitOfWork = unitOfWork;

    // ===========================< METHODS >===========================
    public async Task<ResultRs<AccountRs>> GetByPasswordAsync(AuthRq authRequest)
    {
        // Validate email
        if (BoolUtils.IsValidEmail(authRequest.Email) == false)
            return ResultRs<AccountRs>.Failure("Invalid Email Format!");

        // Check account in database
        var existAccount = await _unitOfWork.GetRepository<Account>()
            .GetOneAsync(acc => acc.Email.Equals(authRequest.Email));

        // Validate password with hashed password using custom method for security
        if (existAccount == null || BoolUtils.VerifyPassword(authRequest.Password, existAccount.PasswordHash) == false)
            return ResultRs<AccountRs>.Failure("Invalid email or password!");

        // Call helper to generate new refresh token and time
        var (token, time) = GenerateNewRefreshToken();

        existAccount.RefreshToken = token;
        existAccount.RefreshTokenExpirytime = time;

        return (await _unitOfWork.CompleteAsync() > 0)
            ? ResultRs<AccountRs>.Ok(existAccount.Adapt<AccountRs>()) : ResultRs<AccountRs>.Failure();
    }

    public async Task<ResultRs<AccountRs>> RefreshTokenAsync(Guid id, string refreshToken)
    {
        var account = await _unitOfWork.GetRepository<Account>().GetOneAsync(u => u.Id == id);

        // Validate for refreshing expired access token
        if (account is null || account.RefreshToken != refreshToken || account.RefreshTokenExpirytime <= DateTime.Now)
            return ResultRs<AccountRs>.BadRequest("Refresh Token is expired or invalid. Please login again.");

        // Call helper to generate new refresh token and time
        var (token, time) = GenerateNewRefreshToken();

        // Generate new refresh token info
        account!.RefreshToken = token;
        account.RefreshTokenExpirytime = account!.RefreshTokenExpirytime ?? time;// After expire days -> must login to get new expire

        return (await _unitOfWork.CompleteAsync() > 0)
            ? ResultRs<AccountRs>.Ok(account.Adapt<AccountRs>()) : ResultRs<AccountRs>.Failure();
    }

    public async Task<ResultRs<bool>> RevokeRefreshTokenAsync(Guid accountId)
    {
        var account = await _unitOfWork.GetRepository<Account>().GetByIdAsync(accountId);
        if (account != null)
        {
            account.RefreshToken = null;
            account.RefreshTokenExpirytime = null;
        }
        return (await _unitOfWork.CompleteAsync() > 0)
            ? ResultRs<bool>.OkBool(true) : ResultRs<bool>.Failure();
    }

    public async Task<ResultRs<bool>> ChangePasswordAsync(string email, string newPassword)
    {
        var account = await _unitOfWork.GetRepository<Account>().GetOneAsync(acc => acc.Email.Equals(email));
        if (account == null)
            return ResultRs<bool>.NotFound("Account not found!");

        // Update for new queried account
        account.PasswordHash = StringUtils.HashPassword(newPassword);
        account.RefreshToken = null;
        account.RefreshTokenExpirytime = null;

        return (await _unitOfWork.CompleteAsync() > 0)
            ? ResultRs<bool>.OkBool(true) : ResultRs<bool>.Failure();
    }

    // ----------------------------< CRUD >----------------------------
    public async Task<ResultRs<bool>> CreateAccountAsync(AuthRq request)
    {
        var accountRepo = _unitOfWork.GetRepository<Account>();

        // 1. Validate exist mail
        bool isEmailExist = await accountRepo.AnyAsync(a => a.Email.Equals(request.Email));
        if (isEmailExist)
            return ResultRs<bool>.Conflict("This email already been used!");

        // 2. Add default to Database
        await accountRepo.AddAsync(new Account
        {
            UserName = StringUtils.GetUsername(request.Email),
            Email = request.Email,
            PasswordHash = StringUtils.HashPassword(request.Password),
        });

        return (await _unitOfWork.CompleteAsync() > 0)
            ? ResultRs<bool>.OkBool(true) : ResultRs<bool>.Failure();
    }

    public async Task<ResultRs<AccountRs>> GetAccountAsync(Guid id, bool includeBadge = false)
    {
        var account = (includeBadge == false)
            ? await _unitOfWork.GetRepository<Account>().GetByIdAsync(id)
            : await _unitOfWork.GetRepository<Account>().GetOneAsync(
                a => a.Id == id,
                q => q.Include(a => a.AccountBadges).ThenInclude(ab => ab.Badge),
                hasTracking: false
            );

        if (account is null) return ResultRs<AccountRs>.NotFound("Not found this account match id!");

        return ResultRs<AccountRs>.Ok(account.Adapt<AccountRs>());
    }

    public async Task<ResultRs<PagedResult<AccountRs>>> GetAccountsPageAsync(PagingQueryRq<AccountQr> input)
    {
        var pageNumber = input.PageNumber > 0 ? input.PageNumber : 1;
        var pageSize = input.PageSize > 0 ? input.PageSize : 10;
        var advanceInput = input.AdvanceInput;

        // Query form builder
        var predicate = PredicateBuilder.New<Account>(true);

        // ------------------------------------------
        if (!string.IsNullOrWhiteSpace(input.Keyword))
        {
            // Replace Contain() to use GIN pg_trgm
            predicate = predicate.And(a => EF.Functions.ILike(a.UserName, $"%{input.Keyword}%"));
            predicate = predicate.And(a => EF.Functions.ILike(a.Email, $"%{input.Keyword}%"));
        }

        if (advanceInput is not null)
        {
            // IsVerified
            if (advanceInput.IsVerified.HasValue)
                predicate = predicate.And(a => a.IsVerified == advanceInput.IsVerified);

            // Gender
            predicate = advanceInput.Gender switch
            {
                1 => (ExpressionStarter<Account>)predicate.And(a => a.Gender == true),
                2 => (ExpressionStarter<Account>)predicate.And(a => a.Gender == false),
                3 => (ExpressionStarter<Account>)predicate.And(a => a.Gender == null),
                _ => predicate
            };

            // Nationality
            if (!string.IsNullOrWhiteSpace(advanceInput.Nationality))
                predicate = predicate.And(a => a.Nationality!.Equals(advanceInput.Nationality));

            // JoinedDate
            if (advanceInput.FromDate.HasValue)
                predicate = predicate.And(a => a.JoinedDate >= advanceInput.FromDate);
            if (advanceInput.ToDate.HasValue)
                predicate = predicate.And(a => a.JoinedDate < advanceInput.ToDate.Value.AddDays(1));

            // Role
            if (!string.IsNullOrWhiteSpace(advanceInput.Role))
                predicate = predicate.And(a => a.Role.Equals(advanceInput.Role));

            // Status
            if (!string.IsNullOrWhiteSpace(advanceInput.Status))
                predicate = predicate.And(a => a.Status.Equals(advanceInput.Status));
        }

        // Order By Create Date
        static IQueryable<Account> OrderByDate(IQueryable<Account> query) => query.OrderByDescending(a => a.JoinedDate);

        // ------------------------------------------
        var accountRepo = _unitOfWork.GetRepository<Account>();
        var accounts = await accountRepo.GetPagedAsync(pageNumber, pageSize, predicate, OrderByDate);

        return accounts.Any() ?
            ResultRs<PagedResult<AccountRs>>.Ok(
                new PagedResult<AccountRs>
                {
                    TotalCount = await accountRepo.CountAsync(predicate),
                    PageSize = input.PageSize,
                    PageIndex = input.PageNumber,
                    DataList = accounts.Adapt<IEnumerable<AccountRs>>()
                }) :
            ResultRs<PagedResult<AccountRs>>.NotFound();
    }

    public async Task<ResultRs<bool>> UpdateAccountAsync(AccountRq request, bool updateAll = true)
    {
        var accountRepo = _unitOfWork.GetRepository<Account>();

        var existAccount = await accountRepo.GetOneAsync(a => a.Id == request.Id, hasTracking: true);

        if (existAccount == null) return ResultRs<bool>.NotFound("Not found this Id account!");

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

        // Cannot Update Password through this method. Use ResetPassword instead
        existAccount.PasswordHash = temp.PasswordHash;

        return (await _unitOfWork.CompleteAsync() > 0)
             ? ResultRs<bool>.OkBool(true) : ResultRs<bool>.Failure();
    }

    public async Task<ResultRs<bool>> DeleteAccountAsync(Guid id)
    {
        var accountRepo = _unitOfWork.GetRepository<Account>();

        var existAccount = await accountRepo.GetByIdAsync(id);
        if (existAccount == null)
            return ResultRs<bool>.NotFound("Not found this Id account!");

        // Soft delete
        existAccount.Status = "Deleted";

        return (await _unitOfWork.CompleteAsync() > 0)
            ? ResultRs<bool>.OkBool(true) : ResultRs<bool>.Failure();
    }

    public async Task<ResultRs<bool>> DeletePermanentAccountAsync(Guid id)
    {
        var accountRepo = _unitOfWork.GetRepository<Account>();

        var existAccount = await accountRepo.GetByIdAsync(id);
        if (existAccount == null)
            return ResultRs<bool>.NotFound("Not found this Id account!");

        // Hard Delete
        await _unitOfWork.GetRepository<Account>().DeleteAsync(existAccount);

        return (await _unitOfWork.CompleteAsync() > 0)
            ? ResultRs<bool>.OkBool(true) : ResultRs<bool>.Failure();
    }

    // ----------------------------[ Helper Method ]----------------------------
    private static (string token, DateTime time) GenerateNewRefreshToken()
    {
        // Get expire limit day in env
        var exDay = int.TryParse(Environment.GetEnvironmentVariable("JWT_REFRESH_TOKEN_EXPIRATION_DAYS"), out var d)
        ? d : 7;
        return (StringUtils.GenerateRefreshToken(), DateTime.Now.AddDays(exDay));
    }
}