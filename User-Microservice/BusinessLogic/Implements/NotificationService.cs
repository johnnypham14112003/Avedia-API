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

public class NotificationService(IUnitOfWork unitOfWork) : INotificationService
{
    private readonly IUnitOfWork _unitOfWork = unitOfWork;

    // ===========================< METHODS >===========================
    /// <summary>
    /// Create as a Notification only, not create many AccountNotification for Global for better performace.<para/>
    /// When want to mark as read, just create AccountNotification as read
    /// </summary>
    public async Task<ResultRs<bool>> CreateGlobalNotificationAsync(NotificationRq request)
    {
        var notiRepo = _unitOfWork.GetRepository<Notification>();

        var newNotification = request.Adapt<Notification>();
        newNotification.Id = Guid.NewGuid();
        newNotification.CreatedDate = DateOnly.FromDateTime(DateTime.Now);
        newNotification.IsGlobal = true;

        await notiRepo.AddAsync(newNotification);

        return (await _unitOfWork.CompleteAsync() > 0)
            ? ResultRs<bool>.OkBool(true) : ResultRs<bool>.Failure();
    }

    public async Task<ResultRs<bool>> CreatePersonalNotificationAsync(NotificationRq request, IEnumerable<Guid> accountIds)
    {
        if (accountIds == null || !accountIds.Any())
            return ResultRs<bool>.BadRequest("List user receive notification cannot be null!");

        var notiRepo = _unitOfWork.GetRepository<Notification>();
        var accNotiRepo = _unitOfWork.GetRepository<AccountNotification>();

        var newNotification = request.Adapt<Notification>();
        newNotification.Id = Guid.NewGuid();
        newNotification.CreatedDate = DateOnly.FromDateTime(DateTime.Now);
        newNotification.IsGlobal = false;

        await notiRepo.AddAsync(newNotification);

        // Tạo danh sách AccountNotification
        var accountNotifications = accountIds.Select(accountId => new AccountNotification
        {
            AccountId = accountId,
            NotificationId = newNotification.Id,
            IsRead = false,
            CreatedDate = DateTime.Now
        });

        await accNotiRepo.AddRangeAsync(accountNotifications);

        return (await _unitOfWork.CompleteAsync() > 0)
            ? ResultRs<bool>.OkBool(true) : ResultRs<bool>.Failure();
    }

    public async Task<ResultRs<PagedResult<NotificationRs>>> GetMyNotificationsAsync(PagingQueryRq<NotificationQr> query)
    {
        var pageNumber = query.PageNumber > 0 ? query.PageNumber : 1;
        var pageSize = query.PageSize > 0 ? query.PageSize : 10;

        var filter = query.AdvanceInput;

        var notificationRepo = _unitOfWork.GetRepository<Notification>();
        IEnumerable<Notification>? notifications;

        // Default: true => GetAll
        var predicate = PredicateBuilder.New<Notification>(true);

        // Search by keyword
        if (!string.IsNullOrWhiteSpace(query.Keyword))
            predicate = predicate.And(n => EF.Functions.ILike(n.Title, $"%{query.Keyword}%"));// Replace Contain() to use GIN pg_trgm
        
        // If have filter
        if (filter is not null)
        {
            // By Type
            if (!string.IsNullOrWhiteSpace(filter.Type))
                predicate = predicate.And(n => n.Type == filter.Type);

            // By TypeId
            if (filter.TypeId.HasValue)
                predicate = predicate.And(n => n.TypeId == filter.TypeId.Value);

            // By CreatedDate
            if (filter.FromDate.HasValue)
                predicate = predicate.And(n => n.CreatedDate >= filter.FromDate.Value);

            if (filter.ToDate.HasValue)
                predicate = predicate.And(n => n.CreatedDate <= filter.ToDate.Value);

            // By Global
            if (filter.IsGlobal.HasValue)
                predicate = predicate.And(n => n.IsGlobal == filter.IsGlobal);

            // If query with Account Id
            if (filter.AccountId is not null && filter.AccountId != Guid.Empty)
            {
                // Get Global and user Noti
                predicate = predicate.And(n => n.IsGlobal || n.AccountNotifications.Any(an => an.AccountId == filter.AccountId));

                // Query include AccNoti for mapping IsRead
                notifications = await notificationRepo.GetPagedAsync(pageNumber, pageSize,
                    include: q => q.Include(n => n.AccountNotifications.Where(an => an.AccountId == filter.AccountId)),
                    predicate: predicate);

                // -------------------------------
                // Map AccNoti to NotiRs
                var mappedNotificationRs = notifications.Select(n =>
                {
                    // Map basic properties
                    var rs = n.Adapt<NotificationRs>();

                    // If AccNoti null (not read Global <=> not create AccNoti) => false
                    rs.IsRead = n.AccountNotifications.FirstOrDefault()?.IsRead ?? false;

                    return rs;
                });

                return notifications.Any() ? ResultRs<PagedResult<NotificationRs>>.Ok(
                    new PagedResult<NotificationRs>
                    {
                        TotalCount = await notificationRepo.CountAsync(predicate),
                        PageSize = pageSize,
                        PageIndex = pageNumber,
                        DataList = mappedNotificationRs
                    })
                    : ResultRs<PagedResult<NotificationRs>>.NotFound();
            }
        }

        // Order By Create Date
        static IQueryable<Notification> OrderByDate(IQueryable<Notification> query) => query.OrderByDescending(n => n.CreatedDate);

        // Normal query without Account Id
        notifications = await notificationRepo.GetPagedAsync(pageNumber, pageSize, predicate, OrderByDate);

        return notifications.Any() ? ResultRs<PagedResult<NotificationRs>>.Ok(
            new PagedResult<NotificationRs>
            {
                TotalCount = await notificationRepo.CountAsync(predicate),
                PageSize = query.PageSize,
                PageIndex = query.PageNumber,
                DataList = notifications.Adapt<IEnumerable<NotificationRs>>()
            })
            : ResultRs<PagedResult<NotificationRs>>.NotFound();
    }

    public async Task<ResultRs<bool>> MarkAsReadAsync(Guid accountId, Guid notificationId)
    {
        var accNotiRepo = _unitOfWork.GetRepository<AccountNotification>();

        var existMapping = await accNotiRepo.GetOneAsync(
            an => an.AccountId == accountId && an.NotificationId == notificationId,
            hasTracking: true);

        // If not created accNoti -> create as global notification has read
        if (existMapping == null)
        {
            // Check if the global noti is exist
            var isGlobalNotification = await _unitOfWork.GetRepository<Notification>()
                .AnyAsync(n => n.Id == notificationId && n.IsGlobal);

            if (!isGlobalNotification)
                return ResultRs<bool>.NotFound("Not found this global notification by id!");

            // Create new read AccountNotification
            existMapping = new AccountNotification
            {
                AccountId = accountId,
                NotificationId = notificationId,
                IsRead = true,
                CreatedDate = DateTime.Now
            };
            await accNotiRepo.AddAsync(existMapping);
        }
        // Already create AccNoti -> change status read
        else
        {
            if (existMapping.IsRead)
                return ResultRs<bool>.OkBool(true);

            existMapping.IsRead = true;
        }

        return (await _unitOfWork.CompleteAsync() > 0)
             ? ResultRs<bool>.OkBool(true) : ResultRs<bool>.Failure();
    }

    public async Task<ResultRs<bool>> MarkAllAsReadAsync(Guid accountId)
    {
        var accNotiRepo = _unitOfWork.GetRepository<AccountNotification>();

        // Get all unread accNoti of this user
        var unreadNotifications = await accNotiRepo.GetListAsync(
            an => an.AccountId == accountId && !an.IsRead,
            hasTracking: true);

        // If not found -> Read all done
        if (unreadNotifications == null || unreadNotifications.Count == 0)
            return ResultRs<bool>.OkBool(true);

        // Loop each unread -> change to read
        foreach (var mapping in unreadNotifications)
        {
            mapping.IsRead = true;
        }

        return (await _unitOfWork.CompleteAsync() > 0)
             ? ResultRs<bool>.OkBool(true) : ResultRs<bool>.Failure();
    }

    public async Task<ResultRs<bool>> DeleteNotificationAsync(Guid notificationId)
    {
        var notiRepo = _unitOfWork.GetRepository<Notification>();
        var accNotiRepo = _unitOfWork.GetRepository<AccountNotification>();

        var existNotification = await notiRepo.GetByIdAsync(notificationId);
        if (existNotification == null)
            return ResultRs<bool>.NotFound("Not found this Id notification!");

        // Delete accNoti reference
        var relatedAccountNotis = await accNotiRepo.GetListAsync(an => an.NotificationId == notificationId);
        if (relatedAccountNotis != null && relatedAccountNotis.Count != 0)
            await accNotiRepo.DeleteRangeAsync(relatedAccountNotis);

        await notiRepo.DeleteAsync(existNotification);

        return (await _unitOfWork.CompleteAsync() > 0)
            ? ResultRs<bool>.OkBool(true) : ResultRs<bool>.Failure();
    }
}