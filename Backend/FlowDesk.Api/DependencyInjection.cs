using FlowDesk.Application;
using FlowDesk.Application.Common.Validators;
using FlowDesk.Domain;
using FlowDesk.Domain.DTOs;
using FlowDesk.Domain.Utils;
using FlowDesk.Infrastructure;
using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
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

                // Custom response for unauthorized and forbidden requests
                options.Events = new JwtBearerEvents
                {
                    OnChallenge = async context =>
                    {
                        context.HandleResponse();

                        context.Response.StatusCode = 401;
                        context.Response.ContentType = "application/json";

                        await context.Response.WriteAsJsonAsync(new ErrorResponseDto
                        {
                            StatusCode = 401,
                            Message = "Unauthorized",
                            ErrorCode = "UNAUTHORIZED",
                            TraceId = context.HttpContext.TraceIdentifier
                        });
                    },
                    OnForbidden = async context =>
                    {
                        context.Response.StatusCode = 403;
                        context.Response.ContentType = "application/json";

                        await context.Response.WriteAsJsonAsync(new ErrorResponseDto
                        {
                            StatusCode = 403,
                            Message = "Forbidden",
                            ErrorCode = "FORBIDDEN",
                            TraceId = context.HttpContext.TraceIdentifier
                        });
                    }
                };

            });

            // Add policy for role-based authorization
            services.AddAuthorization(options =>
            {
                options.AddPolicy("RequireAdminRole", policy => policy.RequireRole(RoleName.Admin));
                options.AddPolicy("RequireManagerRole", policy => policy.RequireRole(RoleName.Manager));
                options.AddPolicy("RequireSupportRole", policy => policy.RequireRole(RoleName.Support));
                options.AddPolicy("RequireEmployeeRole", policy => policy.RequireRole(RoleName.Employee));
                options.AddPolicy("RequireManagerOrAdminRole", policy => policy.RequireRole(RoleName.Manager, RoleName.Admin));
                options.AddPolicy("RequireSupportOrAdminRole", policy => policy.RequireRole(RoleName.Support, RoleName.Admin));
                options.AddPolicy("RequireApprovel", policy => policy.RequireRole(RoleName.Manager));
                options.AddPolicy("RequireAnyRole", policy => policy.RequireRole(RoleName.Admin, RoleName.Manager, RoleName.Support, RoleName.Employee));
            });

            // Add cors policy
            services.AddCors(options =>
            {
                options.AddPolicy("AllowFrontend", policy =>
                {
                    policy
                    .WithOrigins("http://localhost:5173", "https://localhost:5173")
                    .AllowAnyMethod()
                    .AllowCredentials()
                    .AllowAnyHeader();
                });
            });

            return services;
        }
    }
}
