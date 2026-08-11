using BusinessLogic.DTOs.ElasticDocuments;
using BusinessLogic.DTOs.Generic;
using BusinessLogic.DTOs.Messages;
using BusinessLogic.DTOs.Messages.Request;
using BusinessLogic.DTOs.Messages.Request.Query;
using BusinessLogic.DTOs.Messages.Response;
using BusinessLogic.Interfaces;
using DataAccess.Interfaces;
using DataAccess.Models;
using Elastic.Clients.Elasticsearch;
using Elastic.Clients.Elasticsearch.QueryDsl;
using Mapster;
using Microsoft.EntityFrameworkCore;

namespace BusinessLogic.Implements;

public class ActorService(IUnitOfWork unitOfWork, ElasticsearchClient elasticClient) : IActorService
{
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private readonly ElasticsearchClient _elasticClient = elasticClient;

    // ===========================< METHODS >===========================
    public async Task<ResultRs<bool>> CreateActorAsync(ActorRq request)
    {
        var actorRepo = _unitOfWork.GetRepository<Actor>();

        var newActor = request.Adapt<Actor>();
        newActor.Id = Guid.NewGuid();

        newActor.Status = "Active";

        await actorRepo.AddAsync(newActor);

        return (await _unitOfWork.CompleteAsync() > 0)
            ? ResultRs<bool>.OkBool(true)
            : ResultRs<bool>.Failure();
    }

    public async Task<ResultRs<ActorRs>> GetActorDetailAsync(Guid actorId)
    {
        var actorRepo = _unitOfWork.GetRepository<Actor>();

        var actor = await actorRepo.GetOneAsync(
            expression: a => a.Id == actorId,
            include: q => q.Include(a => a.ActorImages)
                           .Include(a => a.VideoActors),
            hasTracking: false
        );

        if (actor is null)
            return ResultRs<ActorRs>.NotFound("Not found this actor!");

        return ResultRs<ActorRs>.Ok(actor.Adapt<ActorRs>());
    }

    public async Task<ResultRs<PagedResult<ActorRs>>> GetActorsPageAsync(PagingQueryRq<ActorQr> input)
    {
        var pageNumber = input.PageNumber > 0 ? input.PageNumber : 1;
        var pageSize = input.PageSize > 0 ? input.PageSize : 20;
        var advanceInput = input.AdvanceInput;

        var searchRequest = new SearchRequest<ActorDocument>("actors")
        {
            From = (pageNumber - 1) * pageSize,
            Size = pageSize,
        };

        // Create list for Query action
        var mustQueries = new List<Query>();
        var filterQueries = new List<Query>();

        // 1. By Keyword
        if (!string.IsNullOrWhiteSpace(input.Keyword))
        {
            mustQueries.Add(new MultiMatchQuery
            {
                Fields = new[] { "fullName", "stageName" },
                Query = input.Keyword,
                Fuzziness = new Fuzziness("AUTO")
            });
        }
        else
        {
            // Get All if no keyword
            mustQueries.Add(new MatchAllQuery());
        }

        // 2. By filter
        if (advanceInput is not null)
        {
            if (advanceInput.Gender.HasValue)
                filterQueries.Add(new TermQuery { Field = "gender", Value = advanceInput.Gender.Value });

            if (!string.IsNullOrWhiteSpace(advanceInput.YearOfBirth))
                filterQueries.Add(new DateRangeQuery
                {
                    Field = "dob",
                    Gte = $"{advanceInput.YearOfBirth}-01-01",
                    Lte = $"{advanceInput.YearOfBirth}-12-31"
                });

            if (!string.IsNullOrWhiteSpace(advanceInput.Height))
                filterQueries.Add(new TermQuery { Field = "height", Value = advanceInput.Height });

            if (!string.IsNullOrWhiteSpace(advanceInput.CupSize))
                filterQueries.Add(new TermQuery { Field = "cupSize", Value = advanceInput.CupSize });

            if (!string.IsNullOrWhiteSpace(advanceInput.Size))
                filterQueries.Add(new TermQuery { Field = "size", Value = advanceInput.Size });

            if (!string.IsNullOrWhiteSpace(advanceInput.Nationality))
                filterQueries.Add(new TermQuery { Field = "nationality", Value = advanceInput.Nationality });

            if (!string.IsNullOrWhiteSpace(advanceInput.Company))
                filterQueries.Add(new TermQuery { Field = "company", Value = advanceInput.Company });

            if (!string.IsNullOrWhiteSpace(advanceInput.Status))
                filterQueries.Add(new TermQuery { Field = "status", Value = advanceInput.Status });

            // By Debut date (Date Range)
            if (advanceInput.FromDebutDate.HasValue || advanceInput.ToDebutDate.HasValue)
            {
                var dateRange = new DateRangeQuery { Field = "debutDate" };

                if (advanceInput.FromDebutDate.HasValue)
                    dateRange.Gte = advanceInput.FromDebutDate.Value.ToString("yyyy-MM-dd");

                if (advanceInput.ToDebutDate.HasValue)
                    dateRange.Lte = advanceInput.ToDebutDate.Value.ToString("yyyy-MM-dd");

                filterQueries.Add(dateRange);
            }
        }

        // Create Bool Query to assign filter into searchRequest
        searchRequest.Query = new BoolQuery
        {
            Must = mustQueries.Count > 0 ? mustQueries : null,
            Filter = filterQueries.Count > 0 ? filterQueries : null
        };

        // 3. Handle Sort
        var sortOptions = new List<SortOptions>();

        if (advanceInput?.OrderBy is > 0)
        {
            switch (advanceInput.OrderBy)
            {
                case 1: // A - Z
                    sortOptions.Add(new SortOptions { Field = new FieldSort { Field = "fullName", Order = SortOrder.Asc } });
                    break;
                case 2: // Z - A
                    sortOptions.Add(new SortOptions { Field = new FieldSort { Field = "fullName", Order = SortOrder.Desc } });
                    break;
                case 3: // Oldest
                    sortOptions.Add(new SortOptions { Field = new FieldSort { Field = "dob", Order = SortOrder.Asc } });
                    break;
                case 4: // Youngest first
                    sortOptions.Add(new SortOptions { Field = new FieldSort { Field = "dob", Order = SortOrder.Desc } });
                    break;
                case 5: // Oldest
                    sortOptions.Add(new SortOptions { Field = new FieldSort { Field = "debutDate", Order = SortOrder.Asc } });
                    break;
                case 6: // Newest
                    sortOptions.Add(new SortOptions { Field = new FieldSort { Field = "debutDate", Order = SortOrder.Desc } });
                    break;
            }
        }
        // Default sort list by debut date
        else if (string.IsNullOrWhiteSpace(input.Keyword))
        {
            sortOptions.Add(new SortOptions { Field = new FieldSort { Field = "debutDate", Order = SortOrder.Desc } });
        }

        if (sortOptions.Count != 0)
        {
            searchRequest.Sort = sortOptions;
        }

        // 4. Execute Query
        var response = await _elasticClient.SearchAsync<ActorDocument>(searchRequest);

        if (!response.IsValidResponse || response.Documents.Count == 0)
        {
            return ResultRs<PagedResult<ActorRs>>.NotFound();
        }

        // 5. Return mapping object
        return ResultRs<PagedResult<ActorRs>>.Ok(new PagedResult<ActorRs>
        {
            TotalCount = (int)response.Total,
            PageSize = pageSize,
            PageIndex = pageNumber,
            DataList = response.Documents.Adapt<IEnumerable<ActorRs>>()
        });
    }

    public async Task<ResultRs<bool>> UpdateActorAsync(ActorRq request)
    {
        var actorRepo = _unitOfWork.GetRepository<Actor>();

        // Query exist Actor
        var existActor = await actorRepo.GetOneAsync(a => a.Id == request.Id, hasTracking: true);

        if (existActor is null)
            return ResultRs<bool>.NotFound("Not found this actor!");

        // Map new data to exist data
        request.Adapt(existActor);

        return (await _unitOfWork.CompleteAsync() > 0)
             ? ResultRs<bool>.OkBool(true)
             : ResultRs<bool>.Failure();
    }

    public async Task<ResultRs<bool>> DeleteActorAsync(Guid actorId)
    {
        var actorRepo = _unitOfWork.GetRepository<Actor>();
        var existActor = await actorRepo.GetByIdAsync(actorId);

        if (existActor == null)
            return ResultRs<bool>.NotFound("Not found this actor!");

        // Soft delete
        existActor.Status = "Deleted";

        // Hard Delete
        //await actorRepo.DeleteAsync(existActor);

        return (await _unitOfWork.CompleteAsync() > 0)
            ? ResultRs<bool>.OkBool(true)
            : ResultRs<bool>.Failure();
    }
}
