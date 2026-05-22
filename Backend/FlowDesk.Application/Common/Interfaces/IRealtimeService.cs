using System;
using System.Collections.Generic;
using System.Text;

namespace FlowDesk.Application.Common.Interfaces
{
    public interface IRealtimeService
    {
        Task NotifyRequestCreatedAsync(int requestId, int employeeId, int? managerId);
        Task NotifyRequestUpdatedAsync(int requestId, string action, IEnumerable<int> userIds);
    }
}
