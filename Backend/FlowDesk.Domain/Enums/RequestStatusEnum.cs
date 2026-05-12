using System;
using System.Collections.Generic;
using System.Text;

namespace FlowDesk.Domain.Enums
{
    public enum RequestStatusEnum
    {
        Open = 1,
        PendingApproval = 2,
        Approved = 3,
        Rejected = 4,
        Assigned = 5,
        InProgress = 6,
        Resolved = 7,
        Closed = 8
    }
}
