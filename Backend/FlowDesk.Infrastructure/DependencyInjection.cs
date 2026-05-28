using FlowDesk.Application.Features.Chat.Interfaces;
using FlowDesk.Domain.Interfaces;
using FlowDesk.Infrastructure.Data;
using FlowDesk.Infrastructure.Repositories;
using FlowDesk.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using FlowDesk.Application.Common.Interfaces;
namespace FlowDesk.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {

            services.AddDbContext<AppDbContext>(options =>
                options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));

            services.AddScoped<IJwtTokenService, JwtTokenService>();

            services.AddScoped<IRefreshTokenService, RefreshTokenService>();

            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<IRoleRepository, RoleRepository>();
            services.AddScoped<IRequestRepository, RequestRepository>();
            services.AddScoped<IRequestHistoryRepository, RequestHistoryRepository>();
            services.AddScoped<ICommentRepository, CommentRepository>();
            services.AddScoped<ICategoryRepository, CategoryRepository>();
            services.AddScoped<IUnitOfWork, UnitOfWork>();
            services.AddScoped<IRequestsService, RequestsService>();

            services.AddScoped<IFileParser, FileParser>();

            services.Configure<GroqSettings>(configuration.GetSection(GroqSettings.SectionName));

            services.AddHttpClient("GroqApi")
                .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
                {
                    MaxConnectionsPerServer = 5
                });

            services.AddScoped<IChatbotService, GroqChatService>();

            return services;
        }
    }
}
