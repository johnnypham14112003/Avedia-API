using AppGrpc.Protos;
using BusinessLogic.DTOs.Generic;
using BusinessLogic.DTOs.Messages.Request;
using BusinessLogic.DTOs.Messages.Request.Query;
using BusinessLogic.Interfaces;
using Grpc.Core;
using Mapster;

namespace AppGrpc.Services;

public class VideoGrpcEndpoint(IVideoService videoService) : VideoGrpcService.VideoGrpcServiceBase
{
    private readonly IVideoService _videoService = videoService;

    public override async Task<VideoResponse> CreateVideo(VideoRequest request, ServerCallContext context)
    {
        var result = await _videoService.CreateVideoAsync(request.Adapt<VideoRq>());

        return new VideoResponse
        {
            ResultResponse = result.Adapt<ResultResponse>(),
            VideoInfo = result.Data.Adapt<VideoInfo>()
        };
    }

    public override async Task<VideoResponse> GetVideoDetail(VideoGetter request, ServerCallContext context)
    {
        var isParsed = Guid.TryParse(request.Id, out var parsedId);
        if (isParsed == false || parsedId == Guid.Empty)
            return new VideoResponse { ResultResponse = new ResultResponse { HttpCode = 400, ErrorMessage = "ID invalid!" } };

        var result = await _videoService.GetVideoDetailAsync(parsedId);

        return new VideoResponse
        {
            ResultResponse = result.Adapt<ResultResponse>(),
            VideoInfo = result.Data.Adapt<VideoInfo>()
        };
    }

    public override async Task<VideoPageResponse> GetVideosPage(VideoPageRequest request, ServerCallContext context)
    {
        VideoQr? advanceInput = null;
        var pageQuery = request.PageQueryRequest;

        // Pre-check message before map to service method's param
        if (request.AdvanceInput != null)
        {
            advanceInput = request.AdvanceInput.Adapt<VideoQr>();
            if (!string.IsNullOrEmpty(request.AdvanceInput.FromReleaseDate))
                advanceInput.FromReleaseDate = DateOnly.Parse(request.AdvanceInput.FromReleaseDate);

            if (!string.IsNullOrEmpty(request.AdvanceInput.ToReleaseDate))
                advanceInput.ToReleaseDate = DateOnly.Parse(request.AdvanceInput.ToReleaseDate);
        }

        // ==========[ Mapping PROTO -> DTOs ]==========
        var queryInput = new PagingQueryRq<VideoQr>
        {
            Keyword = pageQuery.Keyword,
            PageNumber = pageQuery.PageNumber,
            PageSize = pageQuery.PageSize,
            AdvanceInput = advanceInput
        };

        // Call Repository Method to query in database
        var pagedResult = await _videoService.GetVideosPageAsync(queryInput);

        // ==========[ Mapping DATA -> PROTO ]==========
        // Create proto message [VideoPageResponse](0)
        var response = new VideoPageResponse
        {
            // Assign to proto message [ResultResponse](1)
            ResultResponse = pagedResult.Adapt<ResultResponse>()
        };

        if (pagedResult.Data != null)
        {
            // Create proto message [VideoPagedResult](2*)
            var pagedDataProto = new VideoPageResponse.Types.VideoPagedResult
            {
                // Assign to proto message [BasePageResult](2.1)
                BasePageResult = new BasePageResult
                {
                    PageIndex = pagedResult.Data.PageIndex,
                    PageSize = pagedResult.Data.PageSize,
                    TotalCount = pagedResult.Data.TotalCount,
                    TotalPage = pagedResult.Data.TotalPage
                }
            };

            // Assign to proto message [repeated VideoInfo](2.2)
            if (pagedResult.Data.DataList != null && pagedResult.Data.DataList.Any())
            {
                // VideoRs:cs -> VideoInfo:proto
                var mappedList = pagedResult.Data.DataList.Adapt<IEnumerable<VideoInfo>>();
                pagedDataProto.DataList.AddRange(mappedList);
            }
            // Assign to proto message [VideoPagedResult](2)
            response.PagedData = pagedDataProto;
        }

        return response;
    }

    public override async Task<VideoResponse> UpdateVideo(VideoRequest request, ServerCallContext context)
    {
        var result = await _videoService.UpdateVideoAsync(request.VideoInfo.Adapt<VideoRq>());

        return new VideoResponse
        {
            ResultResponse = result.Adapt<ResultResponse>()
        };
    }

    public override async Task<VideoResponse> DeleteVideo(VideoGetter request, ServerCallContext context)
    {
        var isParsed = Guid.TryParse(request.Id, out var parsedId);
        if (isParsed == false || parsedId == Guid.Empty)
            return new VideoResponse { ResultResponse = new ResultResponse { HttpCode = 400, ErrorMessage = "ID invalid!" } };

        var result = await _videoService.DeleteVideoAsync(parsedId);

        return new VideoResponse
        {
            ResultResponse = result.Adapt<ResultResponse>()
        };
    }
}
