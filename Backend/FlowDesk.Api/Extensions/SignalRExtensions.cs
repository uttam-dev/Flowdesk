using FlowDesk.Api.Services.Realtime;
using FlowDesk.Application.Common.Interfaces;

namespace FlowDesk.Api.Extensions;

public static class SignalRExtensions
{
    public static IServiceCollection AddSignalRServices(this IServiceCollection services)
    {
        services.AddSignalR();
        services.AddScoped<IRealtimeService, SignalRService>();

        return services;
    }
}