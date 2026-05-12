using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;

namespace FlowDesk.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            return services;
        }
    }
}
