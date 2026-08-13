using AppGrpc.Protos;
using BusinessLogic.DTOs.Generic;
using BusinessLogic.DTOs.Messages.Request;
using BusinessLogic.DTOs.Messages.Request.Query;
using BusinessLogic.Interfaces;
using Grpc.Core;
using Mapster;

namespace AppGrpc.Services;

public class GenreGrpcEndpoint(IGenreService genreService) : GenreGrpcService.GenreGrpcServiceBase
{
    private readonly IGenreService _genreService = genreService;

    public override async Task<GenreResponse> CreateGenre(GenreRequest request, ServerCallContext context)
    {
        var result = await _genreService.CreateGenreAsync(request.Adapt<GenreRq>());

        return new GenreResponse
        {
            ResultResponse = result.Adapt<ResultResponse>(),
            GenreInfo = result.Data.Adapt<GenreInfo>()
        };
    }

    public override async Task<GenreResponse> GetGenreDetail(GenreGetter request, ServerCallContext context)
    {
        var isParsed = Guid.TryParse(request.Id, out var parsedId);
        if (isParsed == false || parsedId == Guid.Empty)
            return new GenreResponse { ResultResponse = new ResultResponse { HttpCode = 400, ErrorMessage = "ID invalid!" } };

        var result = await _genreService.GetGenreDetailAsync(parsedId);

        return new GenreResponse
        {
            ResultResponse = result.Adapt<ResultResponse>(),
            GenreInfo = result.Data.Adapt<GenreInfo>()
        };
    }

    public override async Task<GenreListResponse> GetGenresList(GenreGetter request, ServerCallContext context)
    {
        // Call Repository Method to query in database
        var listResult = await _genreService.GetAllGenresAsync(request.IncludeDelete);

        // ==========[ Mapping DATA -> PROTO ]==========
        // Create proto message [GenreListResponse](0)
        var response = new GenreListResponse
        {
            // Assign to proto message [ResultResponse](1)
            ResultResponse = listResult.Adapt<ResultResponse>()
        };

        // Assign to proto message [repeated GenreInfo](2)
        if (listResult.Data != null && listResult.Data.Any())
        {
            // GenreRs:cs -> GenreInfo:proto
            var mappedList = listResult.Data.Adapt<IEnumerable<GenreInfo>>();
            response.DataList.AddRange(mappedList);
        }

        return response;
    }

    public override async Task<GenreResponse> UpdateGenre(GenreRequest request, ServerCallContext context)
    {
        var result = await _genreService.UpdateGenreAsync(request.GenreInfo.Adapt<GenreRq>());

        return new GenreResponse
        {
            ResultResponse = result.Adapt<ResultResponse>()
        };
    }

    public override async Task<GenreResponse> DeleteGenre(GenreGetter request, ServerCallContext context)
    {
        var isParsed = Guid.TryParse(request.Id, out var parsedId);
        if (isParsed == false || parsedId == Guid.Empty)
            return new GenreResponse { ResultResponse = new ResultResponse { HttpCode = 400, ErrorMessage = "ID invalid!" } };

        var result = await _genreService.DeleteGenreAsync(parsedId);

        return new GenreResponse
        {
            ResultResponse = result.Adapt<ResultResponse>()
        };
    }
}
