using AppGrpc.Protos;
using BusinessLogic.DTOs.Generic;
using BusinessLogic.DTOs.Messages.Request;
using BusinessLogic.DTOs.Messages.Request.Query;
using BusinessLogic.Implements;
using BusinessLogic.Interfaces;
using Grpc.Core;
using Mapster;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace AppGrpc.Services;

public class NotificationGrpcEndpoint(INotificationService notificationService) : NotificationGrpcService.NotificationGrpcServiceBase
{
    private readonly INotificationService _notificationService = notificationService;

    public override async Task<NotificationResponse> CreateGlobalNotification(NotificationRequest request, ServerCallContext context)
    {
        var result = await _notificationService.CreateGlobalNotificationAsync(request.NotificationInfo.Adapt<NotificationRq>());

        return new NotificationResponse
        {
            ResultResponse = result.Adapt<ResultResponse>()
        };
    }

    public override async Task<NotificationResponse> CreatePersonalNotification(NotificationRequest request, ServerCallContext context)
    {
        // Save parse GUID
        var guidList = request.ListNotificationId.Select(id =>
        {
            if (Guid.TryParse(id, out var parsedGuid))
                return parsedGuid;

            throw new RpcException(new Status(StatusCode.InvalidArgument, $"The notification Id '{id}' in list is not a valid GUID."));
        });

        var result = await _notificationService.CreatePersonalNotificationAsync(request.NotificationInfo.Adapt<NotificationRq>(), guidList);

        return new NotificationResponse
        {
            ResultResponse = result.Adapt<ResultResponse>()
        };
    }

    public override async Task<NotificationPageResponse> GetMyNotifications(NotificationPageRequest request, ServerCallContext context)
    {
        // Pre-Validate
        var pageQuery = request.PageQueryRequest;
        var advanceInput = request.AdvanceInput;

        DateOnly? parsedFromDate = null;
        DateOnly? parsedToDate = null;
        Guid accIdQr = Guid.Empty;
        Guid typeIdQr = Guid.Empty;
        if (advanceInput != null)
        {
            if (!string.IsNullOrEmpty(advanceInput.FromDate))
                parsedFromDate = DateOnly.Parse(advanceInput.FromDate);

            if (!string.IsNullOrEmpty(advanceInput.ToDate))
                parsedToDate = DateOnly.Parse(advanceInput.ToDate);

            if (Guid.TryParse(advanceInput.AccountId, out accIdQr) == false || accIdQr == Guid.Empty)
                return new NotificationPageResponse { ResultResponse = new ResultResponse { HttpCode = 400, ErrorMessage = "Account ID invalid!" } };

            if (Guid.TryParse(advanceInput.TypeId, out typeIdQr) == false || typeIdQr == Guid.Empty)
                return new NotificationPageResponse { ResultResponse = new ResultResponse { HttpCode = 400, ErrorMessage = "Type ID invalid!" } };
        }

        var queryInput = new PagingQueryRq<NotificationQr>
        {
            Keyword = pageQuery.Keyword,
            PageNumber = pageQuery.PageNumber,
            PageSize = pageQuery.PageSize,
            AdvanceInput = (advanceInput == null) ? null :
            new NotificationQr
            {
                AccountId = accIdQr,
                TypeId = typeIdQr,
                Type = advanceInput.Type,
                IsGlobal = advanceInput.IsGlobal,
                FromDate = parsedFromDate,
                ToDate = parsedToDate
            }
        };

        var result = await _notificationService.GetMyNotificationsAsync(queryInput);

        var response =  new NotificationPageResponse
        {
            // [ResultResponse](1)
            ResultResponse = result.Adapt<ResultResponse>(),
        };

        if (result.Data != null)
        {
            // [NotificationPagedResult](2)
            var pagedDataProto = new NotificationPageResponse.Types.NotificationPagedResult
            {
                // [BasePageResult](2.1)
                BasePageResult = new BasePageResult
                {
                    PageIndex = result.Data.PageIndex,
                    PageSize = result.Data.PageSize,
                    TotalCount = result.Data.TotalCount,
                    TotalPage = result.Data.TotalPage
                }
            };

            // [repeated NotificationInfo](2.2)
            if (result.Data.DataList != null && result.Data.DataList.Any())
            {
                // NotificationInfo:cs -> NotificationInfo:proto
                var mappedList = result.Data.DataList.Adapt<IEnumerable<NotificationInfo>>();
                pagedDataProto.DataList.AddRange(mappedList);
            }

            // Assign to response var [NotificationPagedResult](2)
            response.PagedData = pagedDataProto;
        }

        return response;
    }

    public override async Task<NotificationResponse> MarkAsRead(NotificationGetter request, ServerCallContext context)
    {
        if (Guid.TryParse(request.AccountId, out var accId) == false || accId == Guid.Empty)
            return new NotificationResponse { ResultResponse = new ResultResponse { HttpCode = 400, ErrorMessage = "ID invalid!" } };
        if (Guid.TryParse(request.NotificationId, out var notiId) == false || notiId == Guid.Empty)
            return new NotificationResponse { ResultResponse = new ResultResponse { HttpCode = 400, ErrorMessage = "ID invalid!" } };

        var result = await _notificationService.MarkAsReadAsync(accId, notiId);

        return new NotificationResponse
        {
            ResultResponse = result.Adapt<ResultResponse>()
        };
    }

    public override async Task<NotificationResponse> MarkAllAsRead(NotificationGetter request, ServerCallContext context)
    {
        if (Guid.TryParse(request.AccountId, out var accId) == false || accId == Guid.Empty)
            return new NotificationResponse { ResultResponse = new ResultResponse { HttpCode = 400, ErrorMessage = "ID invalid!" } };

        var result = await _notificationService.MarkAllAsReadAsync(accId);

        return new NotificationResponse
        {
            ResultResponse = result.Adapt<ResultResponse>()
        };
    }

    public override async Task<NotificationResponse> DeleteNotification(NotificationGetter request, ServerCallContext context)
    {
        if (Guid.TryParse(request.NotificationId, out var notiId) == false || notiId == Guid.Empty)
            return new NotificationResponse { ResultResponse = new ResultResponse { HttpCode = 400, ErrorMessage = "ID invalid!" } };

        var result = await _notificationService.DeleteNotificationAsync(notiId);

        return new NotificationResponse
        {
            ResultResponse = result.Adapt<ResultResponse>()
        };
    }
}