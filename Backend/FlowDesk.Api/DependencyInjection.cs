using FlowDesk.Application;
using FlowDesk.Application.Common.Validators;
using FlowDesk.Domain;
using FlowDesk.Infrastructure;
using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Serilog;
using System.Text;

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

            // add swagger
            services.AddSwaggerGen();

            //Fluent Validation
            services.AddValidatorsFromAssemblyContaining<UserValidator>();
            services.AddFluentValidationAutoValidation();

            //Jwt Authentication config
            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidIssuer = configuration["Jwt:Issuer"],
                    ValidAudience = configuration["Jwt:Audience"],
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["Jwt:Key"]!)),
                    RequireExpirationTime = true,
                    RequireSignedTokens = true
                };
            });

            services.AddAuthorization();

            return services;
        }
    }
}
