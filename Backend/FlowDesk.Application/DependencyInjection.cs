using AutoMapper;
using FlowDesk.Application.Behaviors;
using FlowDesk.Application.Common.Mappings;
using FlowDesk.Application.Features.Chat.Interfaces;
using FlowDesk.Application.Features.Chat.Services;
using FlowDesk.Application.Features.Requests.Commands;
using FlowDesk.Application.Features.Requests.DTOs;
using FlowDesk.Application.Services;
using FlowDesk.Domain.Interfaces;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace FlowDesk.Application
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddApplication(this IServiceCollection services)
        {
            services.AddScoped<IAuthService, AuthService>();
            services.AddScoped<ISlaService, SlaService>();
            services.AddScoped<PasswordService>();

            // Chat - scoped (depend on MediatR/repositories)
            services.AddScoped<ICommandResolver, CommandResolverService>();
            services.AddScoped<IEntityContextBuilder, EntityContextBuilderService>();
            services.AddScoped<ActionExecutorService>();

            // Chat - singleton (in-memory state, no scoped deps)
            services.AddSingleton<IConversationMemory, InMemoryConversationMemory>();
            services.AddSingleton<IRateLimiter, InMemoryRateLimiter>();
            services.AddSingleton<IGroundingGuard, GroundingGuardService>();

            var assembly = typeof(DependencyInjection).Assembly;

            services.AddMediatR(cfg =>
            {
                cfg.RegisterServicesFromAssembly(assembly);
            });

            // ── Pipeline behaviors: fire SignalR events AFTER each handler ────────
            // These run post-execution and never mutate handler logic or results.
            services.AddScoped<IPipelineBehavior<ApproveRequestCommand, Unit>,
                ApproveRequestNotificationBehavior>();
            services.AddScoped<IPipelineBehavior<RejectRequestCommand, Unit>,
                RejectRequestNotificationBehavior>();
            services.AddScoped<IPipelineBehavior<UpdateRequestStatusCommand, Unit>,
                UpdateRequestStatusNotificationBehavior>();
            services.AddScoped<IPipelineBehavior<AssignRequestCommand, Unit>,
                AssignRequestNotificationBehavior>();
            services.AddScoped<IPipelineBehavior<EscalateRequestCommand, RequestResponseDto>,
                EscalateRequestNotificationBehavior>();

            // Auto mapper
            services.AddSingleton<IMapper>(sp =>
            {
                var loggerFactory = sp.GetRequiredService<ILoggerFactory>();

                var config = new MapperConfiguration(cfg =>
                {
                    cfg.AddProfile<MappingProfile>();
                }, loggerFactory);

                return config.CreateMapper();
            });

            return services;
        }
    }
}
