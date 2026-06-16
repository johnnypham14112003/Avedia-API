using BusinessLogic.DTOs.Generic;
using BusinessLogic.DTOs.Messages;
using BusinessLogic.DTOs.Messages.Request;
using BusinessLogic.DTOs.Messages.Response;
using BusinessLogic.Interfaces;
using DataAccess.Interfaces;
using DataAccess.Models;
using Mapster;
using System.Linq.Expressions;

namespace BusinessLogic.Implements;

public class FavoriteService(IUnitOfWork unitOfWork) : IFavoriteService
{
    private readonly IUnitOfWork _unitOfWork = unitOfWork;

    public async Task<ResultRs<bool>> CheckUserFavoritedAsync(FavoriteRq request)
    {
        // Pre validate
        if (request.AccountId is null || request.TargetId is null ||
            request.AccountId == Guid.Empty || request.TargetId == Guid.Empty ||
            string.IsNullOrEmpty(request.TargetType))
            return ResultRs<bool>.BadRequest();

        bool hasFavorited = await _unitOfWork.GetRepository<Favorite>().AnyAsync(f =>
            f.AccountId == request.AccountId &&
            f.TargetType == request.TargetType &&
            f.TargetId == request.TargetId);

        return ResultRs<bool>.OkBool(hasFavorited);
    }

    public async Task<ResultRs<bool>> ToggleFavoriteAsync(FavoriteRq request)
    {
        // Pre validate
        if (request.AccountId is null || request.TargetId is null ||
            request.AccountId == Guid.Empty || request.TargetId == Guid.Empty ||
            string.IsNullOrEmpty(request.TargetType))
            return ResultRs<bool>.BadRequest();

        var favoriteRepo = _unitOfWork.GetRepository<Favorite>();
        var existFavorite = await favoriteRepo.GetOneAsync(f =>
            f.AccountId == request.AccountId &&
            f.TargetType == request.TargetType &&
            f.TargetId == request.TargetId);

        // Delete if exist
        if (existFavorite != null)
        {
            await favoriteRepo.DeleteAsync(existFavorite);
            return (await _unitOfWork.CompleteAsync() > 0)
                ? ResultRs<bool>.OkBool(false) : ResultRs<bool>.Failure();
        }

        // Favorite if not yet
        await favoriteRepo.AddAsync(new Favorite
        {
            AccountId = (Guid)request.AccountId,
            TargetType = request.TargetType,
            TargetId = (Guid)request.TargetId
        });

        return (await _unitOfWork.CompleteAsync() > 0)
            ? ResultRs<bool>.OkBool(true) : ResultRs<bool>.Failure();
    }

    public async Task<ResultRs<PagedResult<FavoriteRs>>> GetUserFavoritesAsync(PagingQueryRq<FavoriteRq> input)
    {
        // Pre validate
        var pageNumber = input.PageNumber > 0 ? input.PageNumber : 1;
        var pageSize = input.PageSize > 0 ? input.PageSize : 10;

        var favoriteRepo = _unitOfWork.GetRepository<Favorite>();
        var query = input.AdvanceInput;

        // If (AdvanceInput is null) -> GetAll
        Expression<Func<Favorite, bool>> predicate = f => true;

        if (query is not null)
        {
            // if any condition is null => true (EntityFramework will skip that condition and move on to next one)
            predicate = f =>
                (query.AccountId == null || query.AccountId == Guid.Empty || f.AccountId == query.AccountId) &&
                (string.IsNullOrEmpty(query.TargetType) || f.TargetType == query.TargetType);
        }

        // Call DB to query
        var favorites = await favoriteRepo.GetPagedAsync(
            pageNumber: pageNumber,
            pageSize: pageSize,
            predicate: predicate
        );

        if (favorites == null || !favorites.Any())
            return ResultRs<PagedResult<FavoriteRs>>.NotFound();

        var totalCount = await favoriteRepo.CountAsync(predicate);

        return ResultRs<PagedResult<FavoriteRs>>.Ok(
        new PagedResult<FavoriteRs>
        {
            TotalCount = totalCount,
            PageSize = pageSize,
            PageIndex = pageNumber,
            DataList = favorites.Adapt<IEnumerable<FavoriteRs>>()
        });
    }

    public async Task<ResultRs<int>> CountTargetFavoriteAsync(string targetType, Guid targetId)
    {
        int count = await _unitOfWork.GetRepository<Favorite>().CountAsync(f =>
            f.TargetType == targetType &&
            f.TargetId == targetId);

        return ResultRs<int>.Ok(count);
    }
}