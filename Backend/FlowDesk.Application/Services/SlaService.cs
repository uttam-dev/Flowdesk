using FlowDesk.Domain.Interfaces;

namespace FlowDesk.Application.Services
{
    public class SlaService : ISlaService
    {
        public string CalculateSlaStatus(DateTime? dueDate, string requestStatus)
        {
            if (requestStatus == "Resolved" || requestStatus == "Closed")
            {
                return "Completed";
            }

            if (dueDate == null)
            {
                return "No SLA";
            }

            var now = DateTime.UtcNow;

            if (now > dueDate)
            {
                return "Breached";
            }

            if (now > dueDate.Value.AddHours(-2))
            {
                return "Nearing Breach";
            }

            return "Within SLA";
        }
    }
}
