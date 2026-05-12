using FlowDesk.Application;
using FlowDesk.Application.Common.Validators;
using FlowDesk.Domain;
using FlowDesk.Infrastructure;
using FluentValidation;
using FluentValidation.AspNetCore;
using Serilog;

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

            //Fluent Validation
            services.AddValidatorsFromAssemblyContaining<UserValidator>();
            services.AddFluentValidationAutoValidation();

            return services;
        }
    }
}
