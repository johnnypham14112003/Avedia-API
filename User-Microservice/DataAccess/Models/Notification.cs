using System;
using System.Collections.Generic;

namespace DataAccess.Models;

public partial class Notification
{
    public Guid Id { get; set; }

    public string? Type { get; set; }

    public Guid? TypeId { get; set; }

    public string Title { get; set; } = null!;

    public string? Message { get; set; }

    public bool IsGlobal { get; set; }

    public DateOnly CreatedDate { get; set; }

    public virtual ICollection<AccountNotification> AccountNotifications { get; set; } = new List<AccountNotification>();
}
