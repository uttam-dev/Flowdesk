namespace FlowDesk.Application.Features.Chat.Interfaces
{
    public interface IGroundingGuard
    {
        string Validate(string aiResponse, string contextData);
    }
}
