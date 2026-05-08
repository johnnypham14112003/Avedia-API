using System;
using System.Collections.Generic;

namespace DataAccess.Models;

public partial class AccountNotification
{
    public Guid AccountId { get; set; }

    public Guid NotificationId { get; set; }

    public bool IsRead { get; set; }

    public DateTime CreatedDate { get; set; }

    public virtual Account Account { get; set; } = null!;

    public virtual Notification Notification { get; set; } = null!;
}
