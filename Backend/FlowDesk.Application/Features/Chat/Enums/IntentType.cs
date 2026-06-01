namespace FlowDesk.Application.Features.Chat.Enums
{
    public enum IntentType
    {
        Help,
        Greeting,
        BotIdentity,
        CreateRequestHelp,
        ExplainStatus,
        ExplainRoles,

        MyRequests,
        OpenRequests,
        PendingApprovals,
        AssignedToMe,
        UserProfile,
        RequestDetail,

        HowToApprove,
        HowToAssign,
        HowToComment,
        HowToEscalate,
        HowToUpdateStatus,
        HowToCreateRequest,

        SlaStatus,
        EscalatedRequests,
        CategoryInfo,
        TeamSummary,
        SystemSummary,
        RequestSummary,

        // ACTION INTENTS
        CreateRequest,
        ApproveRequest,
        RejectRequest,
        StartRequest,
        ResolveRequest,

        GeneralQuery
    }
}
