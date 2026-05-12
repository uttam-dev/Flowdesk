using FlowDesk.Application;
using FlowDesk.Domain;
using FlowDesk.Infrastructure;


namespace FlowDesk.Api
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddApi(this IServiceCollection services, IConfiguration configuration)
        {

            services
            .AddApplication()
            .AddDomain()
            .AddInfrastructure(configuration);

            return services;
        }
    }
}
