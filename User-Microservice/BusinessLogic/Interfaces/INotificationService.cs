using BusinessLogic.DTOs.Generic;
using BusinessLogic.DTOs.Messages;
using BusinessLogic.DTOs.Messages.Request;
using BusinessLogic.DTOs.Messages.Request.Query;
using BusinessLogic.DTOs.Messages.Response;

namespace BusinessLogic.Interfaces;

public interface INotificationService
{
    Task<ResultRs<bool>> CreateGlobalNotificationAsync(NotificationRq request);
    Task<ResultRs<bool>> CreatePersonalNotificationAsync(NotificationRq request, IEnumerable<Guid> accountIds);
    Task<ResultRs<PagedResult<NotificationRs>>> GetMyNotificationsAsync(PagingQueryRq<NotificationQr> query);
    Task<ResultRs<bool>> MarkAsReadAsync(Guid accountId, Guid notificationId);
    Task<ResultRs<bool>> MarkAllAsReadAsync(Guid accountId);
    Task<ResultRs<bool>> DeleteNotificationAsync(Guid notificationId);
}