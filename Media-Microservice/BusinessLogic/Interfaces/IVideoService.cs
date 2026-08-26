using BusinessLogic.DTOs.Generic;
using BusinessLogic.DTOs.Messages;
using BusinessLogic.DTOs.Messages.Request;
using BusinessLogic.DTOs.Messages.Request.Query;
using BusinessLogic.DTOs.Messages.Response;

namespace BusinessLogic.Interfaces;

public interface IVideoService
{
    Task<ResultRs<bool>> CreateVideoAsync(VideoRq request);
    Task<ResultRs<bool>> DeleteVideoAsync(Guid videoId);
    Task<ResultRs<VideoRs>> GetVideoDetailAsync(Guid videoId);
    Task<ResultRs<PagedResult<VideoRs>>> GetVideosPageAsync(PagingQueryRq<VideoQr> input);
    Task<ResultRs<bool>> UpdateVideoAsync(VideoRq request);
}