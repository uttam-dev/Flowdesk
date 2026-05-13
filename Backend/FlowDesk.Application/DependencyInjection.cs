using FlowDesk.Application.Services;
using FlowDesk.Domain.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace FlowDesk.Application
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddApplication(this IServiceCollection services)
        {
            services.AddScoped<IAuthService, AuthService>();
            services.AddScoped<PasswordService>();

            return services;
        }
    }
}
