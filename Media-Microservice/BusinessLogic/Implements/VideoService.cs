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

public class VideoService(IUnitOfWork unitOfWork, ElasticsearchClient elasticClient)
{
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private readonly ElasticsearchClient _elasticClient = elasticClient;

    // ===========================< METHODS >===========================
    public async Task<ResultRs<bool>> CreateVideoAsync(VideoRq request)
    {
        var videoRepo = _unitOfWork.GetRepository<Video>();

        var isDuplicate = await videoRepo.AnyAsync(v => v.Title == request.Title || v.Code == request.Code);
        if (isDuplicate)
        {
            return ResultRs<bool>.Failure("This name or code is existed!");
        }

        // Map data
        var newVideo = request.Adapt<Video>();

        newVideo.Id = Guid.NewGuid();
        newVideo.Status = "Active";

        await videoRepo.AddAsync(newVideo);

        return (await _unitOfWork.CompleteAsync() > 0)
            ? ResultRs<bool>.OkBool(true)
            : ResultRs<bool>.Failure();
    }

    public async Task<ResultRs<VideoRs>> GetVideoDetailAsync(Guid videoId)
    {
        var videoRepo = _unitOfWork.GetRepository<Video>();

        var video = await videoRepo.GetOneAsync(
            expression: v => v.Id == videoId,
            include: q => q.Include(v => v.VideoImages).ThenInclude(vi => vi.Image)
                           .Include(v => v.VideoActors).ThenInclude(va => va.Actor)
                           .Include(v => v.Genres)
                           .Include(v => v.Labels)
                           .Include(v => v.Producers)
                           .Include(v => v.Tags),
            hasTracking: false,
            asSplitQuery: true
        );

        if (video is null)
            return ResultRs<VideoRs>.NotFound("Not found this video!");

        return ResultRs<VideoRs>.Ok(video.Adapt<VideoRs>());
    }

    public async Task<ResultRs<PagedResult<VideoRs>>> GetVideosPageAsync(PagingQueryRq<VideoQr> input)
    {
        var pageNumber = input.PageNumber > 0 ? input.PageNumber : 1;
        var pageSize = input.PageSize > 0 ? input.PageSize : 20;
        var advanceInput = input.AdvanceInput;

        var searchRequest = new SearchRequest<VideoDocument>("videos")
        {
            From = (pageNumber - 1) * pageSize,
            Size = pageSize,
        };

        var mustQueries = new List<Query>();
        var filterQueries = new List<Query>();

        // 1. By Keyword
        if (!string.IsNullOrWhiteSpace(input.Keyword))
        {
            mustQueries.Add(new MultiMatchQuery
            {
                Fields = new[] { "title", "originalTitle", "code" },
                Query = input.Keyword,
                Fuzziness = new Fuzziness("AUTO")
            });
        }
        else
        {
            mustQueries.Add(new MatchAllQuery());
        }

        // 2. By filter
        if (advanceInput is not null)
        {
            // By Genre
            if (advanceInput.GenreId.HasValue)
            {
                filterQueries.Add(new TermQuery
                {
                    Field = "genreIds",
                    Value = advanceInput.GenreId.Value.ToString()
                });
            }

            // By Tag
            if (advanceInput.TagId.HasValue)
            {
                filterQueries.Add(new TermQuery
                {
                    Field = "tagIds",
                    Value = advanceInput.TagId.Value.ToString()
                });
            }

            if (!string.IsNullOrWhiteSpace(advanceInput.Code))
                filterQueries.Add(new TermQuery { Field = "code", Value = advanceInput.Code });

            if (!string.IsNullOrWhiteSpace(advanceInput.Director))
                filterQueries.Add(new TermQuery { Field = "director", Value = advanceInput.Director });

            if (!string.IsNullOrWhiteSpace(advanceInput.Language))
                filterQueries.Add(new TermQuery { Field = "language", Value = advanceInput.Language });

            if (!string.IsNullOrWhiteSpace(advanceInput.Series))
                filterQueries.Add(new TermQuery { Field = "series", Value = advanceInput.Series });

            if (!string.IsNullOrWhiteSpace(advanceInput.Status))
                filterQueries.Add(new TermQuery { Field = "status", Value = advanceInput.Status });

            // By duration 
            if (advanceInput.DurationMinutes.HasValue)
            {
                var durationRange = new NumberRangeQuery { Field = "durationMinutes" };

                switch (advanceInput.DurationMinutes.Value)
                {
                    case 1: // 1hr
                        durationRange.Gte = 45;
                        durationRange.Lte = 75;
                        break;
                    case 2: // 2hr
                        durationRange.Gte = 105;
                        durationRange.Lte = 135;
                        break;
                    case 3: // 4hr
                        durationRange.Gte = 240;
                        break;
                }

                filterQueries.Add(durationRange);
            }

            // By release date
            if (advanceInput.FromReleaseDate.HasValue || advanceInput.ToReleaseDate.HasValue)
            {
                var dateRange = new DateRangeQuery { Field = "releaseDate" };

                if (advanceInput.FromReleaseDate.HasValue)
                    dateRange.Gte = advanceInput.FromReleaseDate.Value.ToString("yyyy-MM-dd");

                if (advanceInput.ToReleaseDate.HasValue)
                    dateRange.Lte = advanceInput.ToReleaseDate.Value.ToString("yyyy-MM-dd");

                filterQueries.Add(dateRange);
            }
        }

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
                case 1: // Title A - Z
                    sortOptions.Add(new SortOptions { Field = new FieldSort { Field = "title.keyword", Order = SortOrder.Asc } });
                    break;
                case 2: // Title Z - A
                    sortOptions.Add(new SortOptions { Field = new FieldSort { Field = "title.keyword", Order = SortOrder.Desc } });
                    break;
                case 3: // Oldest release
                    sortOptions.Add(new SortOptions { Field = new FieldSort { Field = "releaseDate", Order = SortOrder.Asc } });
                    break;
                case 4:
                    sortOptions.Add(new SortOptions { Field = new FieldSort { Field = "releaseDate", Order = SortOrder.Desc } });
                    break;
                case 5: // Shortest duration
                    sortOptions.Add(new SortOptions { Field = new FieldSort { Field = "durationMinutes", Order = SortOrder.Asc } });
                    break;
                case 6: // Longest duration
                    sortOptions.Add(new SortOptions { Field = new FieldSort { Field = "durationMinutes", Order = SortOrder.Desc } });
                    break;
                case 7: // Highest like
                    sortOptions.Add(new SortOptions { Field = new FieldSort { Field = "likeCount", Order = SortOrder.Desc } });
                    break;
            }
        }
        else if (string.IsNullOrWhiteSpace(input.Keyword))
        {
            // Newest release
            sortOptions.Add(new SortOptions { Field = new FieldSort { Field = "releaseDate", Order = SortOrder.Desc } });
        }

        if (sortOptions.Count != 0)
        {
            searchRequest.Sort = sortOptions;
        }

        // 4. Execute Query
        var response = await _elasticClient.SearchAsync<VideoDocument>(searchRequest);

        if (!response.IsValidResponse || response.Documents.Count == 0)
        {
            return ResultRs<PagedResult<VideoRs>>.NotFound();
        }

        // 5. Return mapping object
        return ResultRs<PagedResult<VideoRs>>.Ok(new PagedResult<VideoRs>
        {
            TotalCount = (int)response.Total,
            PageSize = pageSize,
            PageIndex = pageNumber,
            DataList = response.Documents.Adapt<IEnumerable<VideoRs>>()
        });
    }

    public async Task<ResultRs<bool>> UpdateVideoAsync(VideoRq request)
    {
        var videoRepo = _unitOfWork.GetRepository<Video>();

        // Query exist entity with tracking to auto-update via UnitOfWork
        var existVideo = await videoRepo.GetOneAsync(v => v.Id == request.Id, hasTracking: true);

        if (existVideo is null)
            return ResultRs<bool>.NotFound("Not found this video!");

        var isDuplicate = await videoRepo.AnyAsync(v => v.Id != request.Id && (v.Title == request.Title || v.Code == request.Code));
        if (isDuplicate)
        {
            return ResultRs<bool>.Failure("This name or code is existed!");
        }

        // Map new data to exist data
        request.Adapt(existVideo);

        return (await _unitOfWork.CompleteAsync() > 0)
             ? ResultRs<bool>.OkBool(true)
             : ResultRs<bool>.Failure();
    }

    public async Task<ResultRs<bool>> DeleteVideoAsync(Guid videoId)
    {
        var videoRepo = _unitOfWork.GetRepository<Video>();

        var existVideo = await videoRepo.GetByIdAsync(videoId);

        if (existVideo == null)
            return ResultRs<bool>.NotFound("Not found this video!");

        // Soft delete
        existVideo.Status = "Deleted";

        return (await _unitOfWork.CompleteAsync() > 0)
            ? ResultRs<bool>.OkBool(true)
            : ResultRs<bool>.Failure();
    }
}
