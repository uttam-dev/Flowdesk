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
                options.UseMySQL(configuration.GetConnectionString("DefaultConnection")!)
                );

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

            services.AddMemoryCache();
            services.AddSingleton<ICacheService, InMemoryCacheService>();

            services.AddSingleton(sp =>
            {
                var settings = sp.GetRequiredService<Microsoft.Extensions.Options.IOptions<GroqSettings>>().Value;
                var client = new HttpClient();
                client.BaseAddress = new Uri(settings.BaseUrl.TrimEnd('/') + "/");
                client.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", settings.ApiKey);
                client.Timeout = TimeSpan.FromSeconds(settings.TimeoutSeconds);
                return client;
            });

            services.AddSingleton<IChatbotService, GroqChatService>();

            return services;
        }
    }
}
