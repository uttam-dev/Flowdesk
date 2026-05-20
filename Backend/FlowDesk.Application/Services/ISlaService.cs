namespace FlowDesk.Application.Services
{
    public interface ISlaService
    {
        string CalculateSlaStatus(DateTime? dueDate, string requestStatus);
    }
}
