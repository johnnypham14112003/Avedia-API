using BusinessLogic.DTOs.Messages;
using BusinessLogic.DTOs.Messages.Request;
using BusinessLogic.DTOs.Messages.Response;
using BusinessLogic.Extensions.Utils;
using BusinessLogic.Interfaces;
using DataAccess.Interfaces;
using DataAccess.Models;
using Mapster;
using Microsoft.EntityFrameworkCore;

namespace BusinessLogic.Implements;

public class AccountService(IUnitOfWork unitOfWork) : IAccountService
{
    private readonly IUnitOfWork _unitOfWork = unitOfWork;

    // ===========================< METHODS >===========================
    /// <summary>
    ///     This method is similar to login but instead of handling access token, it will just handle refresh token
    /// </summary>
    public async Task<ResultRs<AccountRs>> GetByPasswordAsync(AuthRq authRequest)
    {
        // Validate email
        if (BoolUtils.IsValidEmail(authRequest.Email) == false)
            return ResultRs<AccountRs>.Failure("Invalid Email Format!");

        // Check account in database
        var existAccount = await _unitOfWork.GetRepository<Account>().GetOneAsync(
            acc => acc.Email.Equals(authRequest.Email));

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
        if (account is null ||
            account.RefreshToken != refreshToken ||
            account.RefreshTokenExpirytime <= DateTime.Now)
        {
            ResultRs<AccountRs>.BadRequest("Refresh Token is expired or invalid. Please login again.");
        }

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
            ? ResultRs<bool>.Ok(true) : ResultRs<bool>.Failure();
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
            ? ResultRs<bool>.Ok(true) : ResultRs<bool>.Failure();
    }

    // ----------------------------< CRUD >----------------------------
    public async Task<ResultRs<bool>> CreateAccountAsync(AuthRq request)
    {
        var accountRepo = _unitOfWork.GetRepository<Account>();

        // 1. Validate exist mail
        bool isEmailExist = await accountRepo.AnyAsync(a => a.Email.ToLower() == request.Email.ToLower());
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
            ? ResultRs<bool>.Ok(true) : ResultRs<bool>.Failure();
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

    //public async Task<PagedResult<AccountRs>> GetAccountsPageAsync(PagingQueryRq<AccountQr> input)
    //{
    //    // Query form builder
    //    var predicate = PredicateBuilder.New<Account>(true);

    //    // ------------------------------------------
    //    if (!string.IsNullOrWhiteSpace(input.Keyword))
    //    {
    //        predicate = predicate.And(a => a.UserName.Contains(input.Keyword));
    //        predicate = predicate.And(a => a.Email.Contains(input.Keyword));
    //    }

    //    if (input.AdvanceInput is not null)
    //    {
    //        // IsVerified
    //        if (input.AdvanceInput.IsVerified.HasValue)
    //            predicate = predicate.And(a => a.IsVerified == input.AdvanceInput.IsVerified);

    //        // Gender
    //        switch (input.AdvanceInput.Gender)
    //        {
    //            case 0:
    //                predicate = predicate.And(a => a.Gender == false);
    //                break;
    //            case 1:
    //                predicate = predicate.And(a => a.Gender == true);
    //                break;
    //            case 2:
    //                predicate = predicate.And(a => a.Gender == null);
    //                break;
    //        }

    //        // Nationality
    //        if (!string.IsNullOrWhiteSpace(input.AdvanceInput.Nationality))
    //            predicate = predicate.And(a => a.Nationality!.Equals(input.AdvanceInput.Nationality));

    //        // JoinedDate
    //        if (input.AdvanceInput.FromDate.HasValue)
    //            predicate = predicate.And(a => a.JoinedDate >= input.AdvanceInput.FromDate);
    //        if (input.AdvanceInput.ToDate.HasValue)
    //            predicate = predicate.And(a => a.JoinedDate < input.AdvanceInput.ToDate.Value.AddDays(1));

    //        // Role
    //        if (!string.IsNullOrWhiteSpace(input.AdvanceInput.Role))
    //            predicate = predicate.And(a => a.Role.Equals(input.AdvanceInput.Role));

    //        // Status
    //        if (!string.IsNullOrWhiteSpace(input.AdvanceInput.Status))
    //            predicate = predicate.And(a => a.Status.Equals(input.AdvanceInput.Status));
    //    }
    //    // ------------------------------------------
    //    var accountRepo = _unitOfWork.GetRepository<Account>();
    //    var accounts = (
    //        await accountRepo.GetPagedAsync(
    //            predicate: predicate,
    //            pageNumber: input.PageNumber,
    //            pageSize: input.PageSize)
    //        ).Adapt<IEnumerable<AccountRs>>();

    //    return PagedResult<AccountRs>.Ok(
    //        new PagedResult<AccountRs>
    //        {
    //            TotalCount = await accountRepo.CountAsync(x => true),
    //            PageSize = input.PageSize,
    //            PageIndex = input.PageNumber,
    //            DataList = accounts
    //        });
    //}

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
             ? ResultRs<bool>.Ok(true) : ResultRs<bool>.Failure();
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
            ? ResultRs<bool>.Ok(true) : ResultRs<bool>.Failure();
    }

    private static (string token, DateTime time) GenerateNewRefreshToken()
    {
        // Get expire limit day in env
        var exDay = int.TryParse(Environment.GetEnvironmentVariable("JWT_REFRESH_TOKEN_EXPIRATION_DAYS"), out var d)
        ? d : 7;
        return (StringUtils.GenerateRefreshToken(), DateTime.Now.AddDays(exDay));
    }
}
