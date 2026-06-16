using BusinessLogic.DTOs.Generic;
using BusinessLogic.DTOs.Messages;
using BusinessLogic.DTOs.Messages.Request;
using BusinessLogic.DTOs.Messages.Request.Query;
using BusinessLogic.DTOs.Messages.Response;
using BusinessLogic.Interfaces;
using DataAccess.Interfaces;
using DataAccess.Models;
using LinqKit;
using Mapster;

namespace BusinessLogic.Implements;

public class BadgeService(IUnitOfWork unitOfWork) : IBadgeService
{
    private readonly IUnitOfWork _unitOfWork = unitOfWork;

    public async Task<ResultRs<bool>> CreateBadgeAsync(BadgeRq request)
    {
        var badgeRepo = _unitOfWork.GetRepository<Badge>();

        // 1. Validate: Conflict exist
        bool isTitleExist = await badgeRepo.AnyAsync(b => b.Title.Equals(request.Title));
        if (isTitleExist)
            return ResultRs<bool>.Conflict("This badge title already exists!");

        // 2. Mapster & Add default properties
        var newBadge = request.Adapt<Badge>();
        newBadge.Id = Guid.NewGuid();
        newBadge.CreatedDate = DateOnly.FromDateTime(DateTime.Now);
        newBadge.Status = string.IsNullOrWhiteSpace(request.Status) ? "Available" : request.Status;

        await badgeRepo.AddAsync(newBadge);

        return (await _unitOfWork.CompleteAsync() > 0)
            ? ResultRs<bool>.OkBool(true) : ResultRs<bool>.Failure();
    }

    public async Task<ResultRs<bool>> AddBadgeToUserAsync(AccountBadgeRq request)
    {
        // 1. Validate: exist
        var badge = await _unitOfWork.GetRepository<Badge>().GetByIdAsync(request.BadgeId);
        if (badge is null) return ResultRs<bool>.NotFound("Not found this badge match id!");

        var existAccount = await _unitOfWork.GetRepository<Account>().GetByIdAsync(request.AccountId);
        if (existAccount == null)
            return ResultRs<bool>.NotFound("Not found this Id account!");

        // 2. Mapster & Add default properties
        var newBadge = new AccountBadge
        {
            AccountId = request.AccountId,
            BadgeId = request.BadgeId,
            AwardedDate = DateOnly.FromDateTime(DateTime.Now)
        };

        await _unitOfWork.GetRepository<AccountBadge>().AddAsync(newBadge);

        return (await _unitOfWork.CompleteAsync() > 0)
            ? ResultRs<bool>.OkBool(true) : ResultRs<bool>.Failure();
    }

    public async Task<ResultRs<BadgeRs>> GetBadgeAsync(Guid id)
    {
        var badge = await _unitOfWork.GetRepository<Badge>().GetByIdAsync(id);

        if (badge is null) return ResultRs<BadgeRs>.NotFound("Not found this badge match id!");

        return ResultRs<BadgeRs>.Ok(badge.Adapt<BadgeRs>());
    }

    public async Task<ResultRs<PagedResult<BadgeRs>>> GetBadgesPageAsync(PagingQueryRq<BadgeQr> input)
    {
        var pageNumber = input.PageNumber > 0 ? input.PageNumber : 1;
        var pageSize = input.PageSize > 0 ? input.PageSize : 10;

        // Query form builder
        var predicate = PredicateBuilder.New<Badge>(true);
        if (!string.IsNullOrWhiteSpace(input.Keyword))
            predicate = predicate.And(b => b.Title.Contains(input.Keyword));

        if (input.AdvanceInput is not null)
        {
            // RareLevel
            if (input.AdvanceInput.RareLevel.HasValue)
                predicate = predicate.And(b => b.RareLevel == input.AdvanceInput.RareLevel.Value);

            // Status
            if (!string.IsNullOrWhiteSpace(input.AdvanceInput.Status))
                predicate = predicate.And(b => b.Status.Equals(input.AdvanceInput.Status));

            // CreatedDate Range
            if (input.AdvanceInput.FromDate.HasValue)
                predicate = predicate.And(b => b.CreatedDate >= input.AdvanceInput.FromDate);
            if (input.AdvanceInput.ToDate.HasValue)
                predicate = predicate.And(b => b.CreatedDate <= input.AdvanceInput.ToDate);
        }

        var badgeRepo = _unitOfWork.GetRepository<Badge>();
        var badges = await badgeRepo.GetPagedAsync(pageNumber, pageSize, predicate);

        return badges.Any() ?
            ResultRs<PagedResult<BadgeRs>>.Ok(
                new PagedResult<BadgeRs>
                {
                    TotalCount = await badgeRepo.CountAsync(predicate),
                    PageSize = pageSize,
                    PageIndex = pageNumber,
                    DataList = badges.Adapt<IEnumerable<BadgeRs>>()
                }) :
            ResultRs<PagedResult<BadgeRs>>.NotFound();
    }

    public async Task<ResultRs<bool>> UpdateBadgeAsync(BadgeRq request)
    {
        var badgeRepo = _unitOfWork.GetRepository<Badge>();

        var existBadge = await badgeRepo.GetOneAsync(b => b.Id == request.Id, hasTracking: true);
        if (existBadge is null)
            return ResultRs<bool>.NotFound("Not found this Id badge!");

        // Temp default data
        var tempCreatedDate = existBadge.CreatedDate;

        // Update new data
        request.Adapt(existBadge);

        // Keep the old created date
        existBadge.CreatedDate = tempCreatedDate;

        return (await _unitOfWork.CompleteAsync() > 0)
             ? ResultRs<bool>.OkBool(true) : ResultRs<bool>.Failure();
    }

    public async Task<ResultRs<bool>> DeleteBadgeAsync(Guid id)
    {
        var badgeRepo = _unitOfWork.GetRepository<Badge>();

        var existBadge = await badgeRepo.GetByIdAsync(id);
        if (existBadge == null)
            return ResultRs<bool>.NotFound("Not found this Id badge!");

        // Soft delete
        existBadge.Status = "Deleted";

        return (await _unitOfWork.CompleteAsync() > 0)
            ? ResultRs<bool>.OkBool(true) : ResultRs<bool>.Failure();
    }

    public async Task<ResultRs<bool>> DeletePermanentBadgeAsync(Guid id)
    {
        var badgeRepo = _unitOfWork.GetRepository<Badge>();
        var accountBadgeRepo = _unitOfWork.GetRepository<AccountBadge>();

        var existBadge = await badgeRepo.GetByIdAsync(id);
        if (existBadge == null)
            return ResultRs<bool>.NotFound("Not found this Id badge!");


        // Delete ralate to this badge
        var relatedAccountBadges = await accountBadgeRepo.GetListAsync(ab => ab.BadgeId == id);
        if (relatedAccountBadges != null && relatedAccountBadges.Count != 0)
            await accountBadgeRepo.DeleteRangeAsync(relatedAccountBadges);

        // Delete badge
        await badgeRepo.DeleteAsync(existBadge);

        return (await _unitOfWork.CompleteAsync() > 0)
            ? ResultRs<bool>.OkBool(true) : ResultRs<bool>.Failure();
    }

    public async Task<ResultRs<bool>> RemoveAllBadgesFromAccountAsync(Guid accountId)
    {
        var accountBadgeRepo = _unitOfWork.GetRepository<AccountBadge>();

        var userBadges = await accountBadgeRepo.GetListAsync(ab => ab.AccountId == accountId);
        if (userBadges == null || userBadges.Count == 0)
            return ResultRs<bool>.OkBool(true);

        // Delete relate badage
        await accountBadgeRepo.DeleteRangeAsync(userBadges);

        return (await _unitOfWork.CompleteAsync() > 0)
            ? ResultRs<bool>.OkBool(true) : ResultRs<bool>.Failure();
    }
}