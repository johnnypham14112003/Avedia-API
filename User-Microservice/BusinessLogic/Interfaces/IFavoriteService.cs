using BusinessLogic.DTOs.Generic;
using BusinessLogic.DTOs.Messages;
using BusinessLogic.DTOs.Messages.Request;
using BusinessLogic.DTOs.Messages.Response;

namespace BusinessLogic.Interfaces;

public interface IFavoriteService
{
    Task<ResultRs<bool>> CheckUserFavoritedAsync(FavoriteRq request);
    Task<ResultRs<int>> CountTargetFavoriteAsync(string targetType, Guid targetId);
    /// <summary><![CDATA[
    /// Must put data <FavoriteRq> into]]> [<paramref name="input"/>.AdvanceInput]<![CDATA[
    /// and also the]]> [<paramref name="input"/>.AdvanceInput.AccountId] ; [<paramref name="input"/>.AdvanceInput.TargetType]</summary>
    Task<ResultRs<PagedResult<FavoriteRs>>> GetUserFavoritesAsync(PagingQueryRq<FavoriteRq> input);

    /// <returns><![CDATA[
    /// true / false            <=> favorited / unfavorited
    /// false + HttpCode(422)   => Failed to save change]]></returns>
    Task<ResultRs<bool>> ToggleFavoriteAsync(FavoriteRq request);
}