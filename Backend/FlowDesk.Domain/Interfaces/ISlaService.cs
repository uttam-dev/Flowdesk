namespace FlowDesk.Domain.Interfaces
{
    public interface ISlaService
    {
        string CalculateSlaStatus(DateTime? dueDate, string requestStatus);
    }
}
