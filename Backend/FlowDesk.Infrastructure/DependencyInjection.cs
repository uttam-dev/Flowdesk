using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using FlowDesk.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using FlowDesk.Domain.Interfaces;
using FlowDesk.Infrastructure.Services;
using FlowDesk.Infrastructure.Repositories;

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


            return services;
        }
    }
}
