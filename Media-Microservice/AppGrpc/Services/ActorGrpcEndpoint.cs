using AppGrpc.Protos;
using BusinessLogic.DTOs.Generic;
using BusinessLogic.DTOs.Messages.Request;
using BusinessLogic.DTOs.Messages.Request.Query;
using BusinessLogic.Interfaces;
using Grpc.Core;
using Mapster;

namespace AppGrpc.Services;

public class ActorGrpcEndpoint(IActorService actorService) : ActorGrpcService.ActorGrpcServiceBase
{
    private readonly IActorService _actorService = actorService;

    public override async Task<ActorResponse> CreateActor(ActorRequest request, ServerCallContext context)
    {
        var result = await _actorService.CreateActorAsync(request.Adapt<ActorRq>());

        return new ActorResponse
        {
            ResultResponse = result.Adapt<ResultResponse>(),
            ActorInfo = result.Data.Adapt<ActorInfo>()
        };
    }

    public override async Task<ActorResponse> GetActorDetail(ActorGetter request, ServerCallContext context)
    {
        var isParsed = Guid.TryParse(request.Id, out var parsedId);
        if (isParsed == false || parsedId == Guid.Empty)
            return new ActorResponse { ResultResponse = new ResultResponse { HttpCode = 400, ErrorMessage = "ID invalid!" } };

        var result = await _actorService.GetActorDetailAsync(parsedId);

        return new ActorResponse
        {
            ResultResponse = result.Adapt<ResultResponse>(),
            ActorInfo = result.Data.Adapt<ActorInfo>()
        };
    }

    public override async Task<ActorPageResponse> GetActorsPage(ActorPageRequest request, ServerCallContext context)
    {
        ActorQr? advanceInput = null;
        var pageQuery = request.PageQueryRequest;

        // Pre-check message before map to service method's param
        if (request.AdvanceInput != null)
        {
            advanceInput = request.AdvanceInput.Adapt<ActorQr>();
            if (!string.IsNullOrEmpty(request.AdvanceInput.FromDebutDate))
                advanceInput.FromDebutDate = DateOnly.Parse(request.AdvanceInput.FromDebutDate);

            if (!string.IsNullOrEmpty(request.AdvanceInput.ToDebutDate))
                advanceInput.ToDebutDate = DateOnly.Parse(request.AdvanceInput.ToDebutDate);
        }

        // ==========[ Mapping PROTO -> DTOs ]==========
        var queryInput = new PagingQueryRq<ActorQr>
        {
            Keyword = pageQuery.Keyword,
            PageNumber = pageQuery.PageNumber,
            PageSize = pageQuery.PageSize,
            AdvanceInput = advanceInput
        };

        // Call Repository Method to query in database
        var pagedResult = await _actorService.GetActorsPageAsync(queryInput);

        // ==========[ Mapping DATA -> PROTO ]==========
        // Create proto message [ActorPageResponse](0)
        var response = new ActorPageResponse
        {
            // Assign to proto message [ResultResponse](1)
            ResultResponse = pagedResult.Adapt<ResultResponse>()
        };

        if (pagedResult.Data != null)
        {
            // Create proto message [ActorPagedResult](2*)
            var pagedDataProto = new ActorPageResponse.Types.ActorPagedResult
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

            // Assign to proto message [repeated ActorInfo](2.2)
            if (pagedResult.Data.DataList != null && pagedResult.Data.DataList.Any())
            {
                // ActorRs:cs -> ActorInfo:proto
                var mappedList = pagedResult.Data.DataList.Adapt<IEnumerable<ActorInfo>>();
                pagedDataProto.DataList.AddRange(mappedList);
            }
            // Assign to proto message [ActorPagedResult](2)
            response.PagedData = pagedDataProto;
        }

        return response;
    }

    public override async Task<ActorResponse> UpdateActor(ActorRequest request, ServerCallContext context)
    {
        var result = await _actorService.UpdateActorAsync(request.ActorInfo.Adapt<ActorRq>());

        return new ActorResponse
        {
            ResultResponse = result.Adapt<ResultResponse>()
        };
    }

    public override async Task<ActorResponse> DeleteActor(ActorGetter request, ServerCallContext context)
    {
        var isParsed = Guid.TryParse(request.Id, out var parsedId);
        if (isParsed == false || parsedId == Guid.Empty)
            return new ActorResponse { ResultResponse = new ResultResponse { HttpCode = 400, ErrorMessage = "ID invalid!" } };

        var result = await _actorService.DeleteActorAsync(parsedId);

        return new ActorResponse
        {
            ResultResponse = result.Adapt<ResultResponse>()
        };
    }
}