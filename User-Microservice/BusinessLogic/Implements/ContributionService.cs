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
using Microsoft.EntityFrameworkCore;

namespace BusinessLogic.Implements;

public class ContributionService(IUnitOfWork unitOfWork) : IContributionService
{
    private readonly IUnitOfWork _unitOfWork = unitOfWork;

    public async Task<ResultRs<bool>> CreateContributionAsync(ContributionRq request)
    {
        var contributionRepo = _unitOfWork.GetRepository<Contribution>();

        var newContribution = request.Adapt<Contribution>();

        newContribution.Id = Guid.NewGuid();
        newContribution.RequestedDate = DateOnly.FromDateTime(DateTime.Now);

        newContribution.Status = "Pending";
        newContribution.AdminApproved = false;
        newContribution.ApproverId = null;

        await contributionRepo.AddAsync(newContribution);

        return (await _unitOfWork.CompleteAsync() > 0)
            ? ResultRs<bool>.OkBool(true) : ResultRs<bool>.Failure();
    }

    public async Task<ResultRs<ContributionRs>> GetContributionAsync(Guid contributionId)
    {
        var contribution = await _unitOfWork.GetRepository<Contribution>().GetByIdAsync(contributionId);

        if (contribution is null)
            return ResultRs<ContributionRs>.NotFound("Not found this contribution match id!");

        return ResultRs<ContributionRs>.Ok(contribution.Adapt<ContributionRs>());
    }

    public async Task<ResultRs<PagedResult<ContributionRs>>> GetContributionsPageAsync(PagingQueryRq<ContributionQr> input)
    {
        // Default valid value
        var pageNumber = input.PageNumber > 0 ? input.PageNumber : 1;
        var pageSize = input.PageSize > 0 ? input.PageSize : 10;
        var advanceInput = input.AdvanceInput;

        // Create LINQ Query Builder
        var predicate = PredicateBuilder.New<Contribution>(true);

        // 1. By keyword
        if (!string.IsNullOrWhiteSpace(input.Keyword))
        {
            // Replace Contain() to use GIN pg_trgm
            predicate = predicate.And(c => EF.Functions.ILike(c.TargetType, $"%{input.Keyword}%")
                                        || EF.Functions.ILike(c.ActionType, $"%{input.Keyword}%"));
        }

        if (advanceInput is not null)
        {
            // By Contributor
            if (advanceInput.ContributorId.HasValue)
                predicate = predicate.And(c => c.ContributorId == advanceInput.ContributorId.Value);

            // By TargetId
            if (advanceInput.TargetId.HasValue)
                predicate = predicate.And(c => c.TargetId == advanceInput.TargetId.Value);

            // By AdminApproved
            if (advanceInput.AdminApproved.HasValue)
                predicate = predicate.And(c => c.AdminApproved == advanceInput.AdminApproved.Value);

            // By Status
            if (!string.IsNullOrWhiteSpace(advanceInput.Status))
                predicate = predicate.And(c => c.Status.Equals(advanceInput.Status));

            // By Time
            if (advanceInput.ByTypeDate.HasValue)
            {
                if (advanceInput.ByTypeDate.Value == false)// false: Filter by RequestedDate
                {
                    if (advanceInput.FromDate.HasValue)
                        predicate = predicate.And(c => c.RequestedDate >= advanceInput.FromDate);
                    if (advanceInput.ToDate.HasValue)
                        predicate = predicate.And(c => c.RequestedDate <= advanceInput.ToDate);
                }
                else// true: Filter by HandledDate
                {
                    if (advanceInput.FromDate.HasValue)
                        predicate = predicate.And(c => c.HandledDate >= advanceInput.FromDate);
                    if (advanceInput.ToDate.HasValue)
                        predicate = predicate.And(c => c.HandledDate <= advanceInput.ToDate);
                }
            }
        }

        // Local Function: reuse "include input param" as "order"
        IQueryable<Contribution> OrderByQuery(IQueryable<Contribution> query)
        {
            // If filter by HandledDate -> Order by latest HandledDate
            if (advanceInput?.ByTypeDate == true)
                return query.OrderByDescending(c => c.HandledDate);

            // Default filter by RequestedDate -> Order by latest RequestedDate
            return query.OrderByDescending(c => c.RequestedDate);
        }

        // Start Call DB
        var contributionRepo = _unitOfWork.GetRepository<Contribution>();
        var contributions = await contributionRepo.GetPagedAsync(pageNumber, pageSize, predicate, OrderByQuery);

        // Map return Data
        return contributions.Any() ?
            ResultRs<PagedResult<ContributionRs>>.Ok(
                new PagedResult<ContributionRs>
                {
                    TotalCount = await contributionRepo.CountAsync(predicate),
                    PageSize = pageSize,
                    PageIndex = pageNumber,
                    DataList = contributions.Adapt<IEnumerable<ContributionRs>>()
                }) :
            ResultRs<PagedResult<ContributionRs>>.NotFound();
    }

    public async Task<ResultRs<bool>> UpdateContributionAsync(ContributionRq request)
    {
        var contributionRepo = _unitOfWork.GetRepository<Contribution>();

        // Query model need update
        var existContribution = await contributionRepo.GetOneAsync(c => c.Id == request.Id, hasTracking: true);
        if (existContribution == null)
            return ResultRs<bool>.NotFound("Not found this Id contribution!");

        // If reviewed -> Cannot Change ProposeData
        if (existContribution.AdminApproved)
            return ResultRs<bool>.Conflict("Cannot update a contribution that has already been reviewed!");

        // Create temp constant data
        var tempRequestedDate = existContribution.RequestedDate;
        var tempContributorId = existContribution.ContributorId;

        // Apply new change to tracked model
        request.Adapt(existContribution);
        existContribution.HandledDate = DateOnly.FromDateTime(DateTime.Now);

        // Keep core identity fields that should not change on update
        existContribution.RequestedDate = tempRequestedDate;
        existContribution.ContributorId = tempContributorId;

        return (await _unitOfWork.CompleteAsync() > 0)
             ? ResultRs<bool>.OkBool(true) : ResultRs<bool>.Failure();
    }

    public async Task<ResultRs<bool>> DeleteContributionAsync(Guid id)
    {
        var contributionRepo = _unitOfWork.GetRepository<Contribution>();
        var existContribution = await contributionRepo.GetByIdAsync(id);

        if (existContribution == null)
            return ResultRs<bool>.NotFound("Not found this Id contribution!");

        // Validate: cannot delete if admin approved
        if (existContribution.AdminApproved)
            return ResultRs<bool>.Conflict("Cannot delete a contribution that has already been reviewed!");

        // Hard delete
        await contributionRepo.DeleteAsync(existContribution);

        return (await _unitOfWork.CompleteAsync() > 0)
            ? ResultRs<bool>.OkBool(true) : ResultRs<bool>.Failure();
    }

    // --------------------------------------------------------------------------------------------------------------
    public async Task<ResultRs<bool>> StatusContributionAsync(Guid contributionId, string newStatus)
    {
        var contributionRepo = _unitOfWork.GetRepository<Contribution>();
        var existContribution = await contributionRepo.GetByIdAsync(contributionId);
        if (existContribution == null)
            return ResultRs<bool>.NotFound("Not found this Id contribution!");

        // Validate if admin approved -> cannot Reject
        if (existContribution.AdminApproved)
            return ResultRs<bool>.Conflict("Cannot reject a contribution that has already been reviewed!");

        // Update status & time
        existContribution.HandledDate = DateOnly.FromDateTime(DateTime.Now);
        existContribution.Status = newStatus;

        return (await _unitOfWork.CompleteAsync() > 0)
            ? ResultRs<bool>.OkBool(true) : ResultRs<bool>.Failure();
    }

    public async Task<ResultRs<bool>> ReviewContributionAsync(Guid contributionId, Guid approverId)
    {
        var contributionRepo = _unitOfWork.GetRepository<Contribution>();
        var existContribution = await contributionRepo.GetOneAsync(c => c.Id == contributionId, hasTracking: true);
        if (existContribution == null)
            return ResultRs<bool>.NotFound("Not found this Id contribution!");

        // Check if approved
        if (existContribution.AdminApproved)
            return ResultRs<bool>.Conflict("This contribution has already been reviewed!");

        // Update status
        existContribution.HandledDate = DateOnly.FromDateTime(DateTime.Now);
        existContribution.ApproverId = approverId;
        existContribution.AdminApproved = true;
        existContribution.Status = "Updating";

        return (await _unitOfWork.CompleteAsync() > 0)
            ? ResultRs<bool>.OkBool(true) : ResultRs<bool>.Failure();
    }
}
